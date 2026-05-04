USE ProductDB;
GO

CREATE OR ALTER PROCEDURE usp_GetProductsPaged
    @Page        INT            = 1,
    @PageSize    INT            = 10,
    @CategoryId  INT            = NULL,
    @MinPrice    DECIMAL(18,2)  = NULL,
    @MaxPrice    DECIMAL(18,2)  = NULL,
    @Search      NVARCHAR(200)  = NULL,
    @TotalCount  INT            OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.IsActive = 1
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@MinPrice   IS NULL OR p.Price >= @MinPrice)
      AND (@MaxPrice   IS NULL OR p.Price <= @MaxPrice)
      AND (@Search     IS NULL
           OR p.Name        LIKE '%' + @Search + '%'
           OR p.Description LIKE '%' + @Search + '%'
           OR p.SKU         LIKE '%' + @Search + '%');

    -- Return paged results
    SELECT
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.StockQuantity,
        p.SKU,
        p.ImageUrl,
        p.CategoryId,
        c.Name  AS CategoryName,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.IsActive = 1
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@MinPrice   IS NULL OR p.Price >= @MinPrice)
      AND (@MaxPrice   IS NULL OR p.Price <= @MaxPrice)
      AND (@Search     IS NULL
           OR p.Name        LIKE '%' + @Search + '%'
           OR p.Description LIKE '%' + @Search + '%'
           OR p.SKU         LIKE '%' + @Search + '%')
    ORDER BY p.CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO