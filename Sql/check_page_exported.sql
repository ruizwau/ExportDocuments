SELECT COUNT(1)
FROM dbo.ExportManifest
WHERE BatchNumber = @BatchNumber 
  AND PageIndex = @PageIndex 
  AND Success = 1;