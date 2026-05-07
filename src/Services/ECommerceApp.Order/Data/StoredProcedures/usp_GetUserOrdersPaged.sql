USE OrderDB;
GO

CREATE OR ALTER PROCEDURE usp_GetUserOrdersPaged
    @UserId     NVARCHAR(450),
    @Page       INT = 1,
    @PageSize   INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Total count for this user
    SELECT @TotalCount = COUNT(*)
    FROM Orders
    WHERE UserId = @UserId;

    -- Paged orders
    SELECT
        Id, UserId, UserEmail, Status,
        TotalAmount, ShippingAddress,
        TrackingNumber, Carrier,
        CreatedAt, UpdatedAt
    FROM Orders
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO