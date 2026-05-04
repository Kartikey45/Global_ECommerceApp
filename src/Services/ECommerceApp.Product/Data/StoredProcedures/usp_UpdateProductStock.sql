USE ProductDB;
GO

CREATE OR ALTER PROCEDURE usp_UpdateProductStock
    @ProductId    INT,
    @RequestedQty INT,
    @Success      BIT          OUTPUT,
    @Message      NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CurrentStock INT;

        -- Lock row to prevent race conditions
        -- (two orders buying last item simultaneously)
        SELECT @CurrentStock = StockQuantity
        FROM Products WITH (UPDLOCK, ROWLOCK)
        WHERE Id = @ProductId
          AND IsActive = 1;

        -- Product not found
        IF @CurrentStock IS NULL
        BEGIN
            SET @Success = 0;
            SET @Message = 'Product not found.';
            ROLLBACK;
            RETURN;
        END

        -- Not enough stock
        IF @CurrentStock < @RequestedQty
        BEGIN
            SET @Success = 0;
            SET @Message = 'Insufficient stock. Available: '
                           + CAST(@CurrentStock AS NVARCHAR);
            ROLLBACK;
            RETURN;
        END

        -- Deduct stock
        UPDATE Products
        SET StockQuantity = StockQuantity - @RequestedQty,
            UpdatedAt     = GETUTCDATE()
        WHERE Id = @ProductId;

        SET @Success = 1;
        SET @Message = 'Stock updated successfully.';

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO