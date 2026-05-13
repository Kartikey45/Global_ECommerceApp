USE AnalyticsDB;
GO

CREATE OR ALTER PROCEDURE usp_SaveOrderAnalytic
    @OrderId     INT,
    @UserId      NVARCHAR(450),
    @UserEmail   NVARCHAR(256),
    @TotalAmount DECIMAL(18,2),
    @ItemCount   INT,
    @Status      NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Skip if already exists (idempotent)
        IF EXISTS (
            SELECT 1 FROM OrderAnalytics
            WHERE OrderId = @OrderId)
        BEGIN
            COMMIT;
            RETURN;
        END

        INSERT INTO OrderAnalytics
        (
            OrderId, UserId, UserEmail,
            TotalAmount, ItemCount,
            Status, PaymentSuccess,
            CreatedAt
        )
        VALUES
        (
            @OrderId, @UserId, @UserEmail,
            @TotalAmount, @ItemCount,
            @Status, 0,
            GETUTCDATE()
        );

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO