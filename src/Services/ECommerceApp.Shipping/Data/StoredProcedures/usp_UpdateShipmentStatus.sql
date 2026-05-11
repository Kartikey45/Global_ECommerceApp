USE ShippingDB;
GO

CREATE OR ALTER PROCEDURE usp_UpdateShipmentStatus
    @ShipmentId   INT,
    @Status       NVARCHAR(50),
    @DeliveredAt  DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        UPDATE Shipments
        SET
            Status      = @Status,
            DeliveredAt =
                CASE
                    WHEN @Status = 'Delivered'
                    THEN GETUTCDATE()
                    ELSE DeliveredAt
                END,
            UpdatedAt   = GETUTCDATE()
        WHERE Id = @ShipmentId;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO