USE OrderDB;
GO

CREATE OR ALTER PROCEDURE usp_CreateOrder
    @UserId          NVARCHAR(450),
    @UserEmail       NVARCHAR(256),
    @TotalAmount     DECIMAL(18,2),
    @ShippingAddress NVARCHAR(500),
    @ItemsJson       NVARCHAR(MAX),
    @OrderId         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY

        -- Insert Order
        INSERT INTO Orders
        (
            UserId, UserEmail, Status,
            TotalAmount, ShippingAddress,
            CreatedAt
        )
        VALUES
        (
            @UserId, @UserEmail, 'Pending',
            @TotalAmount, @ShippingAddress,
            GETUTCDATE()
        );

        SET @OrderId = SCOPE_IDENTITY();

        -- Insert all OrderItems from JSON array
        -- JSON format: [{"productId":1,"productName":"iPhone",
        --               "quantity":2,"unitPrice":999.99,
        --               "totalPrice":1999.98}]
        INSERT INTO OrderItems
        (
            OrderId, ProductId, ProductName,
            Quantity, UnitPrice, TotalPrice
        )
        SELECT
            @OrderId,
            JSON_VALUE(item.value, '$.productId'),
            JSON_VALUE(item.value, '$.productName'),
            JSON_VALUE(item.value, '$.quantity'),
            JSON_VALUE(item.value, '$.unitPrice'),
            JSON_VALUE(item.value, '$.totalPrice')
        FROM OPENJSON(@ItemsJson) AS item;

        COMMIT;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO