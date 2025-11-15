import React, { useState, useEffect } from 'react';
import { ArrowLeft, Filter, Undo, Eye } from 'lucide-react';
import { Button } from './ui/button';
import { Card } from './ui/card';
import { Label } from './ui/label';
import { Input } from './ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Page, HistoryRecord } from '../types';
import { storage } from '../utils/storage';
import { toast } from 'sonner@2.0.3';

interface HistoryPageProps {
  onNavigate: (page: Page) => void;
}

export default function HistoryPage({ onNavigate }: HistoryPageProps) {
  const [history, setHistory] = useState<HistoryRecord[]>([]);
  const [filterOpen, setFilterOpen] = useState(false);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<HistoryRecord | null>(null);
  const [filters, setFilters] = useState({
    type: 'all',
    startDate: '',
    endDate: '',
  });

  useEffect(() => {
    loadHistory();
  }, []);

  const loadHistory = () => {
    const data = storage.getHistory();
    setHistory(data);
  };

  const filteredHistory = React.useMemo(() => {
    return history.filter((record) => {
      // Tip filtresi
      if (filters.type !== 'all' && record.type !== filters.type) {
        return false;
      }

      // Tarih filtresi
      if (filters.startDate && record.date < filters.startDate) {
        return false;
      }
      if (filters.endDate && record.date > filters.endDate) {
        return false;
      }

      return true;
    });
  }, [history, filters]);

  const getTypeLabel = (type: HistoryRecord['type']) => {
    switch (type) {
      case 'stock-add':
        return 'Stok Ekleme';
      case 'stock-remove':
        return 'Stok Çıkarma';
      case 'stock-delete':
        return 'Stok Silme';
      case 'case':
        return 'Vaka';
      case 'checklist':
        return 'Kontrol Listesi';
      default:
        return type;
    }
  };

  const getTypeColor = (type: HistoryRecord['type']) => {
    switch (type) {
      case 'stock-add':
        return 'bg-green-100 text-green-800';
      case 'stock-remove':
        return 'bg-red-100 text-red-800';
      case 'stock-delete':
        return 'bg-orange-100 text-orange-800';
      case 'case':
        return 'bg-blue-100 text-blue-800';
      case 'checklist':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const handleUndo = (record: HistoryRecord) => {
    if (!window.confirm('Bu işlemi geri almak istediğinizden emin misiniz?')) {
      return;
    }

    try {
      if (record.type === 'stock-add') {
        // Stok ekleme işlemini geri al - stoğu sil
        const item = record.details;
        if (item && item.id) {
          storage.deleteStockItem(item.id);
          toast.success('Stok ekleme işlemi geri alındı');
        }
      } else if (record.type === 'stock-remove') {
        // Stok çıkarma işlemini geri al - stoğa geri ekle
        const item = record.details;
        if (item) {
          // Duplicate kontrolü yapma, çünkü geri ekliyoruz
          storage.addStock({
            id: Date.now().toString(),
            materialName: item.materialName,
            serialLotNumber: item.serialLotNumber,
            ubbCode: item.ubbCode || '',
            expiryDate: item.expiryDate || '',
            quantity: item.quantity,
            dateAdded: new Date().toISOString().split('T')[0],
            from: '',
            to: '',
          });
          toast.success('Stok çıkarma işlemi geri alındı');
        }
      } else if (record.type === 'stock-delete') {
        // Stok silme işlemini geri al - stoğa geri ekle
        const item = record.details;
        if (item) {
          storage.addStock({
            id: Date.now().toString(),
            materialName: item.materialName,
            serialLotNumber: item.serialLotNumber,
            ubbCode: item.ubbCode || '',
            expiryDate: item.expiryDate || '',
            quantity: item.quantity,
            dateAdded: new Date().toISOString().split('T')[0],
            from: '',
            to: '',
          });
          toast.success('Stok silme işlemi geri alındı');
        }
      } else if (record.type === 'case') {
        // Vaka işlemini geri al - kullanılan malzemeleri stoğa geri ekle
        const caseDetails = record.details;
        if (caseDetails && caseDetails.materials) {
          caseDetails.materials.forEach((material: any) => {
            storage.addStock({
              id: Date.now().toString() + Math.random(),
              materialName: material.materialName,
              serialLotNumber: material.serialLotNumber,
              ubbCode: material.ubbCode || '',
              expiryDate: '',
              quantity: material.quantity,
              dateAdded: new Date().toISOString().split('T')[0],
              from: '',
              to: '',
            });
          });
          toast.success('Vaka işlemi geri alındı, malzemeler stoğa eklendi');
        }
      }

      // Geçmiş kaydını sil
      storage.removeHistory(record.id);
      loadHistory();
    } catch (error) {
      toast.error('İşlem geri alınırken hata oluştu');
      console.error(error);
    }
  };

  const handleShowDetails = (record: HistoryRecord) => {
    setSelectedRecord(record);
    setDetailsOpen(true);
  };

  const renderDetails = (record: HistoryRecord) => {
    const details = record.details;

    if (record.type === 'stock-add') {
      // Tek ürün ekleme
      if (details && details.materialName) {
        return (
          <div>
            <Label>Eklenen Ürün</Label>
            <Card className="p-3 bg-gray-50 mt-2">
              <div className="space-y-1 text-sm">
                <div><span className="text-slate-600">Malzeme:</span> <span className="text-slate-800">{details.materialName}</span></div>
                <div><span className="text-slate-600">Seri/Lot:</span> <span className="text-slate-800">{details.serialLotNumber}</span></div>
                {details.ubbCode && <div><span className="text-slate-600">UBB:</span> <span className="text-slate-800">{details.ubbCode}</span></div>}
                {details.expiryDate && <div><span className="text-slate-600">SKT:</span> <span className="text-slate-800">{new Date(details.expiryDate).toLocaleDateString('tr-TR')}</span></div>}
                <div><span className="text-slate-600">Miktar:</span> <span className="text-slate-800">{details.quantity}</span></div>
                {details.from && <div><span className="text-slate-600">Kimden:</span> <span className="text-slate-800">{details.from}</span></div>}
                {details.to && <div><span className="text-slate-600">Kime:</span> <span className="text-slate-800">{details.to}</span></div>}
              </div>
            </Card>
          </div>
        );
      }
      // Toplu ekleme (Excel import)
      if (Array.isArray(details)) {
        return (
          <div>
            <Label>Eklenen Ürünler ({details.length} adet)</Label>
            <div className="space-y-2 mt-2 max-h-[400px] overflow-y-auto">
              {details.map((item: any, index: number) => (
                <Card key={index} className="p-3 bg-gray-50">
                  <div className="space-y-1 text-sm">
                    <div className="text-slate-500">#{index + 1}</div>
                    <div><span className="text-slate-600">Malzeme:</span> <span className="text-slate-800">{item.materialName}</span></div>
                    <div><span className="text-slate-600">Seri/Lot:</span> <span className="text-slate-800">{item.serialLotNumber}</span></div>
                    {item.ubbCode && <div><span className="text-slate-600">UBB:</span> <span className="text-slate-800">{item.ubbCode}</span></div>}
                    {item.expiryDate && <div><span className="text-slate-600">SKT:</span> <span className="text-slate-800">{new Date(item.expiryDate).toLocaleDateString('tr-TR')}</span></div>}
                    <div><span className="text-slate-600">Miktar:</span> <span className="text-slate-800">{item.quantity}</span></div>
                  </div>
                </Card>
              ))}
            </div>
          </div>
        );
      }
    }

    if (record.type === 'stock-remove') {
      return (
        <div>
          <Label>Çıkarılan Ürün</Label>
          <Card className="p-3 bg-gray-50 mt-2">
            <div className="space-y-1 text-sm">
              <div><span className="text-slate-600">Malzeme:</span> <span className="text-slate-800">{details.materialName}</span></div>
              <div><span className="text-slate-600">Seri/Lot:</span> <span className="text-slate-800">{details.serialLotNumber}</span></div>
              {details.ubbCode && <div><span className="text-slate-600">UBB:</span> <span className="text-slate-800">{details.ubbCode}</span></div>}
              {details.expiryDate && <div><span className="text-slate-600">SKT:</span> <span className="text-slate-800">{new Date(details.expiryDate).toLocaleDateString('tr-TR')}</span></div>}
              <div><span className="text-slate-600">Miktar:</span> <span className="text-slate-800">{details.quantity}</span></div>
              {details.from && <div><span className="text-slate-600">Kimden:</span> <span className="text-slate-800">{details.from}</span></div>}
              {details.to && <div><span className="text-slate-600">Kime:</span> <span className="text-slate-800">{details.to}</span></div>}
            </div>
          </Card>
        </div>
      );
    }

    if (record.type === 'stock-delete') {
      return (
        <div>
          <Label>Silinen Ürün</Label>
          <Card className="p-3 bg-gray-50 mt-2">
            <div className="space-y-1 text-sm">
              <div><span className="text-slate-600">Malzeme:</span> <span className="text-slate-800">{details.materialName}</span></div>
              <div><span className="text-slate-600">Seri/Lot:</span> <span className="text-slate-800">{details.serialLotNumber}</span></div>
              {details.ubbCode && <div><span className="text-slate-600">UBB:</span> <span className="text-slate-800">{details.ubbCode}</span></div>}
              {details.expiryDate && <div><span className="text-slate-600">SKT:</span> <span className="text-slate-800">{new Date(details.expiryDate).toLocaleDateString('tr-TR')}</span></div>}
              <div><span className="text-slate-600">Miktar:</span> <span className="text-slate-800">{details.quantity}</span></div>
            </div>
          </Card>
        </div>
      );
    }

    if (record.type === 'case') {
      return (
        <div>
          <div className="space-y-3">
            <div>
              <Label>Vaka Bilgileri</Label>
              <Card className="p-3 bg-gray-50 mt-2">
                <div className="space-y-1 text-sm">
                  <div><span className="text-slate-600">Hasta:</span> <span className="text-slate-800">{details.patientName}</span></div>
                  <div><span className="text-slate-600">Doktor:</span> <span className="text-slate-800">{details.doctorName}</span></div>
                  <div><span className="text-slate-600">Hastane:</span> <span className="text-slate-800">{details.hospitalName}</span></div>
                  {details.notes && <div><span className="text-slate-600">Notlar:</span> <span className="text-slate-800">{details.notes}</span></div>}
                </div>
              </Card>
            </div>
            
            <div>
              <Label>Kullanılan Malzemeler ({details.materials?.length || 0} adet)</Label>
              <div className="space-y-2 mt-2 max-h-[300px] overflow-y-auto">
                {details.materials?.map((material: any, index: number) => (
                  <Card key={index} className="p-3 bg-gray-50">
                    <div className="space-y-1 text-sm">
                      <div className="text-slate-500">#{index + 1}</div>
                      <div><span className="text-slate-600">Malzeme:</span> <span className="text-slate-800">{material.materialName}</span></div>
                      <div><span className="text-slate-600">Seri/Lot:</span> <span className="text-slate-800">{material.serialLotNumber}</span></div>
                      {material.ubbCode && <div><span className="text-slate-600">UBB:</span> <span className="text-slate-800">{material.ubbCode}</span></div>}
                      <div><span className="text-slate-600">Miktar:</span> <span className="text-slate-800">{material.quantity}</span></div>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          </div>
        </div>
      );
    }

    if (record.type === 'checklist') {
      const checkedCount = details.patients?.filter((p: any) => p.checked).length || 0;
      const totalCount = details.patients?.length || 0;
      
      const formatHospitalName = (name: string) => {
        // Büyük/küçük harf farkı gözetmeksizin tüm varyasyonları yakala
        return name.replace(/eğitim\s+ve\s+araştırma\s+hastanesi/gi, 'EAH');
      };
      
      return (
        <div>
          <div className="space-y-3">
            <div>
              <Label>Kontrol Listesi Bilgileri</Label>
              <Card className="p-3 bg-gray-50 mt-2">
                <div className="space-y-1 text-sm">
                  <div><span className="text-slate-600">Başlık:</span> <span className="text-slate-800">{details.title}</span></div>
                  <div><span className="text-slate-600">Başlangıç:</span> <span className="text-slate-800">{new Date(details.createdDate).toLocaleDateString('tr-TR')}</span></div>
                  {details.completedDate && <div><span className="text-slate-600">Tamamlanma:</span> <span className="text-slate-800">{new Date(details.completedDate).toLocaleDateString('tr-TR')}</span></div>}
                  <div><span className="text-slate-600">Durum:</span> <span className="text-slate-800">{checkedCount} / {totalCount} kontrol edildi</span></div>
                </div>
              </Card>
            </div>
            
            <div>
              <Label>Hastalar</Label>
              <div className="space-y-2 mt-2 max-h-[300px] overflow-y-auto">
                {details.patients?.map((patient: any, index: number) => (
                  <Card key={patient.id} className={`p-3 ${patient.checked ? 'bg-green-50' : 'bg-gray-50'}`}>
                    <div className="space-y-1 text-sm">
                      <div className="flex items-center gap-2">
                        <span className="text-slate-500">#{index + 1}</span>
                        <span className={patient.checked ? 'text-green-800 line-through' : 'text-slate-800'}>{patient.name}</span>
                        {patient.checked && <span className="text-green-600">✓</span>}
                      </div>
                      {patient.hospital && <div><span className="text-slate-600">Hastane:</span> <span className="text-slate-800">{formatHospitalName(patient.hospital)}</span></div>}
                      {patient.phone && <div><span className="text-slate-600">Tel:</span> <span className="text-slate-800">{patient.phone}</span></div>}
                      {patient.time && <div><span className="text-slate-600">Saat:</span> <span className="text-slate-800">{patient.time}</span></div>}
                      {patient.note && <div><span className="text-slate-600">Not:</span> <span className="text-slate-800">{patient.note}</span></div>}
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          </div>
        </div>
      );
    }

    // Fallback - JSON göster
    return (
      <div>
        <Label>Detaylar</Label>
        <pre className="bg-gray-100 p-4 rounded-lg overflow-x-auto text-slate-700 text-sm mt-2">
          {JSON.stringify(details, null, 2)}
        </pre>
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-purple-600 text-white p-4 sticky top-0 z-20">
        <div className="flex items-center gap-3">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => onNavigate('home')}
            className="text-white hover:bg-purple-700"
          >
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <h1 className="text-white flex-1">Geçmiş</h1>
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setFilterOpen(!filterOpen)}
            className="text-white hover:bg-purple-700"
          >
            <Filter className="w-5 h-5" />
          </Button>
        </div>
      </div>

      {/* Filtreler */}
      {filterOpen && (
        <Card className="m-4 p-4 space-y-3">
          <div>
            <Label>İşlem Tipi</Label>
            <Select
              value={filters.type}
              onValueChange={(value) => setFilters({ ...filters, type: value })}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tümü</SelectItem>
                <SelectItem value="stock-add">Stok Ekleme</SelectItem>
                <SelectItem value="stock-remove">Stok Çıkarma</SelectItem>
                <SelectItem value="stock-delete">Stok Silme</SelectItem>
                <SelectItem value="case">Vaka</SelectItem>
                <SelectItem value="checklist">Kontrol Listesi</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label>Başlangıç Tarihi</Label>
            <Input
              type="date"
              value={filters.startDate}
              onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
            />
          </div>

          <div>
            <Label>Bitiş Tarihi</Label>
            <Input
              type="date"
              value={filters.endDate}
              onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
            />
          </div>

          <Button
            variant="outline"
            className="w-full"
            onClick={() =>
              setFilters({ type: 'all', startDate: '', endDate: '' })
            }
          >
            Filtreleri Temizle
          </Button>
        </Card>
      )}

      {/* Geçmiş Listesi */}
      <div className="p-4">
        {filteredHistory.length === 0 ? (
          <Card className="p-8 text-center">
            <p className="text-slate-500">Geçmiş kayıt bulunamadı</p>
          </Card>
        ) : (
          <div className="space-y-3">
            {filteredHistory.map((record) => (
              <Card key={record.id} className="p-4">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-2">
                      <span
                        className={`px-2 py-1 rounded-full ${getTypeColor(
                          record.type
                        )}`}
                      >
                        {getTypeLabel(record.type)}
                      </span>
                      <span className="text-slate-500">
                        {new Date(record.date).toLocaleDateString('tr-TR')}
                      </span>
                    </div>
                    <p className="text-slate-700">{record.description}</p>
                  </div>
                  <div className="flex gap-1">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleShowDetails(record)}
                      title="Detayları Gör"
                    >
                      <Eye className="w-4 h-4" />
                    </Button>
                    {record.type !== 'checklist' && (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleUndo(record)}
                        title="Geri Al"
                      >
                        <Undo className="w-4 h-4" />
                      </Button>
                    )}
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>

      {/* Detaylar Dialog */}
      <Dialog open={detailsOpen} onOpenChange={setDetailsOpen}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>İşlem Detayları</DialogTitle>
          </DialogHeader>
          {selectedRecord && (
            <div className="space-y-4">
              <div>
                <Label>İşlem Tipi</Label>
                <p className="text-slate-700">{getTypeLabel(selectedRecord.type)}</p>
              </div>
              <div>
                <Label>Tarih</Label>
                <p className="text-slate-700">
                  {new Date(selectedRecord.date).toLocaleDateString('tr-TR')}
                </p>
              </div>
              <div>
                <Label>Açıklama</Label>
                <p className="text-slate-700">{selectedRecord.description}</p>
              </div>
              {renderDetails(selectedRecord)}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
