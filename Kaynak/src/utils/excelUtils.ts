// Excel işlemleri için yardımcı fonksiyonlar
import * as XLSX from 'xlsx';
import { StockItem, ChecklistPatient } from '../types';

// Excel'den veri içe aktar
export const importFromExcel = (file: File): Promise<StockItem[]> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e) => {
      try {
        const data = e.target?.result;
        const workbook = XLSX.read(data, { type: 'binary' });
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];
        const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        const stockItems: StockItem[] = [];

        // İlk satır başlık olduğu için 1'den başla
        for (let i = 1; i < jsonData.length; i++) {
          const row = jsonData[i];
          
          // Boş satırları atla
          if (!row || row.length === 0 || !row[1]) continue;

          const materialCode = row[1]?.toString().trim() || ''; // Malzeme Kodu (A sütunu)
          const materialName = row[2]?.toString().trim() || ''; // Malzeme Açıklaması (B sütunu)
          const descriptionCell = row[3]?.toString().trim() || ''; // Açıklama (C sütunu)
          const quantity = parseInt(row[4]) || 0; // Miktar (D sütunu)

          if (!materialName || quantity <= 0) continue;

          // Açıklama hücresini parse et
          // Format: "LOT:XXXX\SKT:DD.MM.YYYY" veya "SERI:XXXX\SKT:DD.MM.YYYY" veya "LOT:XXXX\SKT:DD.MM.YYYY\UBB:XXXX"
          let serialLotNumber = '';
          let expiryDate = '';
          let ubbCode = '';

          // Backslash ile ayrılmış parçaları ayır
          const parts = descriptionCell.split('\\');
          
          parts.forEach(part => {
            const cleanPart = part.trim();
            
            // LOT numarasını bul
            const lotMatch = cleanPart.match(/^LOT:(.+)$/i);
            if (lotMatch) {
              serialLotNumber = lotMatch[1].trim();
              return;
            }
            
            // SERI numarasını bul
            const seriMatch = cleanPart.match(/^SERI:(.+)$/i);
            if (seriMatch) {
              serialLotNumber = seriMatch[1].trim();
              return;
            }
            
            // SKT tarihini bul
            const sktMatch = cleanPart.match(/^SKT:(.+)$/i);
            if (sktMatch) {
              const dateStr = sktMatch[1].trim();
              // DD.MM.YYYY veya DD/MM/YYYY formatını YYYY-MM-DD'ye çevir
              const dateParts = dateStr.split(/[\/\.]/);
              if (dateParts.length === 3) {
                const day = dateParts[0].padStart(2, '0');
                const month = dateParts[1].padStart(2, '0');
                const year = dateParts[2];
                expiryDate = `${year}-${month}-${day}`;
              }
              return;
            }
            
            // UBB kodunu bul
            const ubbMatch = cleanPart.match(/^UBB:(.+)$/i);
            if (ubbMatch) {
              ubbCode = ubbMatch[1].trim();
              return;
            }
          });

          // Eğer parse edilemezse, tüm açıklama serialLotNumber olarak kaydedilir
          if (!serialLotNumber && descriptionCell) {
            serialLotNumber = descriptionCell;
          }

          const stockItem: StockItem = {
            id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
            materialName,
            serialLotNumber,
            ubbCode,
            expiryDate,
            quantity,
            dateAdded: new Date().toISOString().split('T')[0],
            from: 'Excel İçe Aktarma',
            to: '',
            materialCode: materialCode || undefined, // Malzeme kodunu kaydet
          };

          stockItems.push(stockItem);
        }

        resolve(stockItems);
      } catch (error) {
        reject(new Error('Excel dosyası okunamadı: ' + error));
      }
    };

    reader.onerror = () => {
      reject(new Error('Dosya okunamadı'));
    };

    reader.readAsBinaryString(file);
  });
};

// Stok verilerini Excel'e aktar
export const exportToExcel = (stockItems: StockItem[], filename: string = 'stok_listesi.xlsx') => {
  // Excel verilerini hazırla
  const excelData = stockItems.map((item, index) => {
    // Açıklama sütunu: LOT/SERI ve SKT bilgilerini backslash ile ayır
    const descriptionParts: string[] = [];
    
    // LOT veya SERI numarası ekle
    if (item.serialLotNumber) {
      // Eğer numara sadece rakamlardan oluşuyorsa SERI, değilse LOT olarak kabul et
      // Veya kullanıcının girdiği formatı koru
      const isNumericOnly = /^\d+$/.test(item.serialLotNumber);
      if (isNumericOnly) {
        descriptionParts.push(`SERI:${item.serialLotNumber}`);
      } else {
        descriptionParts.push(`LOT:${item.serialLotNumber}`);
      }
    }
    
    // SKT tarihi ekle
    if (item.expiryDate) {
      const date = new Date(item.expiryDate);
      const formattedDate = `${date.getDate().toString().padStart(2, '0')}.${(date.getMonth() + 1).toString().padStart(2, '0')}.${date.getFullYear()}`;
      descriptionParts.push(`SKT:${formattedDate}`);
    }

    // UBB kodu ekle (varsa)
    if (item.ubbCode) {
      descriptionParts.push(`UBB:${item.ubbCode}`);
    }

    const description = descriptionParts.join('\\');

    return {
      'Sıra': index + 1,
      'Malzeme': item.materialCode || '', // Malzeme kodu
      'Malzeme Açıklaması': item.materialName,
      'Açıklama': description,
      'Miktar': item.quantity,
    };
  });

  // Worksheet oluştur
  const worksheet = XLSX.utils.json_to_sheet(excelData);

  // Sütun genişliklerini ayarla
  worksheet['!cols'] = [
    { wch: 8 },  // Sıra
    { wch: 15 }, // Malzeme (Kod)
    { wch: 30 }, // Malzeme Açıklaması
    { wch: 50 }, // Açıklama
    { wch: 10 }, // Miktar
  ];

  // Workbook oluştur
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Stok Listesi');

  // Dosyayı indir
  XLSX.writeFile(workbook, filename);
};

// Kontrol listesi için Excel'den hasta verisi içe aktar
export const importChecklistFromExcel = (file: File): Promise<ChecklistPatient[]> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (e) => {
      try {
        const data = e.target?.result;
        const workbook = XLSX.read(data, { type: 'binary' });
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];
        const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        const patients: ChecklistPatient[] = [];

// Excel saat değerini formatla (Excel saat değerleri günün kesri olarak gelir)
        const formatExcelTime = (value: any): string => {
        if (!value) return '';

        // String olarak gelirse sayıya dönüştür
        const numValue = parseFloat(value);

  // Eğer geçerli bir sayıysa ve 1'den küçükse, Excel zaman formatıdır
  if (!isNaN(numValue) && numValue < 1) {
    const totalMinutes = Math.round(numValue * 24 * 60);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  }

  // Eğer zaten string ise, :00 saniye kısmını kaldır
  const timeStr = value.toString().trim();
  return timeStr.replace(/:\d{2}$/, ''); // Son :00 kısmını kaldır
};



        // İlk satır başlık olduğu için 1'den başla
        for (let i = 1; i < jsonData.length; i++) {
          const row = jsonData[i];
          
          // Boş satırları atla
          if (!row || row.length === 0 || !row[0]) continue;

          const name = row[0]?.toString().trim() || '';
          const note = row[1]?.toString().trim() || '';
          const phone = row[2]?.toString().trim() || '';
          const city = row[3]?.toString().trim() || '';
          const hospital = row[4]?.toString().trim() || '';
          const date = row[5]?.toString().trim() || '';
          const time = formatExcelTime(row[6]);

          if (!name) continue;

          const patient: ChecklistPatient = {
            id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
            name,
            note: note || undefined,
            phone: phone || undefined,
            city: city || undefined,
            hospital: hospital || undefined,
            date: date || undefined,
            time: time || undefined,
            checked: false,
          };

          patients.push(patient);
        }

        resolve(patients);
      } catch (error) {
        reject(new Error('Excel dosyası okunamadı: ' + error));
      }
    };

    reader.onerror = () => {
      reject(new Error('Dosya okunamadı'));
    };

    reader.readAsBinaryString(file);
  });
};
