USE PaymentDB;
GO

CREATE OR ALTER PROCEDURE usp_SavePayment
    @OrderId       INT,
    @UserId        NVARCHAR(450),
    @UserEmail     NVARCHAR(256),
    @Amount        DECIMAL(18,2),
    @Currency      NVARCHAR(10),
    @Status        NVARCHAR(50),
    @TransactionId NVARCHAR(200) = NULL,
    @FailureReason NVARCHAR(500) = NULL,
    @PaymentId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Prevent duplicate completed payment
        -- for the same order
        IF EXISTS (
            SELECT 1 FROM Payments
            WHERE OrderId = @OrderId
              AND Status  = 'Completed')
        BEGIN
            RAISERROR(
                'Payment already completed for this order.',
                16, 1);
        END

        INSERT INTO Payments
        (
            OrderId, UserId, UserEmail,
            Amount, Currency, Status,
            TransactionId, FailureReason,
            RetryCount, CreatedAt
        )
        VALUES
        (
            @OrderId, @UserId, @UserEmail,
            @Amount, @Currency, @Status,
            @TransactionId, @FailureReason,
            0, GETUTCDATE()
        );

        SET @PaymentId = SCOPE_IDENTITY();
        COMMIT;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO