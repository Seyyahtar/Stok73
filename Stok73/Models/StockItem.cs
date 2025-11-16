using System;

namespace Stok73.Models;

public sealed record StockItem(
    string Id,
    string MaterialName,
    string SerialLotNumber,
    string UbbCode,
    DateTime ExpiryDate,
    int Quantity,
    string Location,
    string CategoryKey,
    string CategoryName,
    string Manufacturer);
