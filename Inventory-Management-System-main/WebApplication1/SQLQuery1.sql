USE [InventoryDB];
GO

-- 1. إضافة قسم تجريبي
INSERT INTO [Categories] ([CategoryName], [Description])
VALUES (N'Electronics', N'Laptops and gadgets');

-- 2. إضافة منتج تجريبي مرتبط بالقسم
INSERT INTO [Products] ([SKU], [ProductName], [CategoryID], [UnitPrice], [StockQuantity], [LowStockThreshold])
VALUES (N'DELL-XPS-001', N'Laptop Dell XPS 15', SCOPE_IDENTITY(), 1500.00, 10, 2);
GO