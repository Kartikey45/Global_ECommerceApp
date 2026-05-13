USE AnalyticsDB;
GO

CREATE OR ALTER PROCEDURE usp_UpdateOrderAnalyticPayment
    @OrderId       INT,
    @PaymentSuccess BIT,
    @RevenueAmount DECIMAL(18,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        UPDATE OrderAnalytics
        SET
            PaymentSuccess = @PaymentSuccess,
            RevenueAmount  = @RevenueAmount,
            Status         =
                CASE
                    WHEN @PaymentSuccess = 1
                    THEN 'Confirmed'
                    ELSE 'PaymentFailed'
                END,
            UpdatedAt      = GETUTCDATE()
        WHERE OrderId = @OrderId;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO