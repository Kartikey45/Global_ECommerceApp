USE PaymentDB;
GO

CREATE OR ALTER PROCEDURE usp_UpdatePaymentStatus
    @PaymentId     INT,
    @Status        NVARCHAR(50),
    @TransactionId NVARCHAR(200) = NULL,
    @FailureReason NVARCHAR(500) = NULL,
    @RetryCount    INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        UPDATE Payments
        SET
            Status        = @Status,
            TransactionId =
                COALESCE(@TransactionId, TransactionId),
            FailureReason =
                COALESCE(@FailureReason, FailureReason),
            RetryCount    =
                COALESCE(@RetryCount, RetryCount),
            UpdatedAt     = GETUTCDATE()
        WHERE Id = @PaymentId;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO