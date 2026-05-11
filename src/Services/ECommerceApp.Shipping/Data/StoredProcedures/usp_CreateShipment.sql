USE ShippingDB;
GO

CREATE OR ALTER PROCEDURE usp_CreateShipment
    @OrderId          INT,
    @UserId           NVARCHAR(450),
    @UserEmail        NVARCHAR(256),
    @TrackingNumber   NVARCHAR(100),
    @Carrier          NVARCHAR(50),
    @DeliveryAddress  NVARCHAR(500),
    @EstimatedDelivery DATETIME2,
    @ShipmentId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY

        -- Prevent duplicate shipment for same order
        IF EXISTS (
            SELECT 1 FROM Shipments
            WHERE OrderId = @OrderId)
        BEGIN
            -- Return existing shipment ID
            SELECT @ShipmentId = Id
            FROM Shipments
            WHERE OrderId = @OrderId;

            COMMIT;
            RETURN;
        END

        INSERT INTO Shipments
        (
            OrderId, UserId, UserEmail,
            TrackingNumber, Carrier,
            Status, DeliveryAddress,
            EstimatedDelivery, ShippedAt,
            CreatedAt
        )
        VALUES
        (
            @OrderId, @UserId, @UserEmail,
            @TrackingNumber, @Carrier,
            'Processing', @DeliveryAddress,
            @EstimatedDelivery, GETUTCDATE(),
            GETUTCDATE()
        );

        SET @ShipmentId = SCOPE_IDENTITY();
        COMMIT;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO