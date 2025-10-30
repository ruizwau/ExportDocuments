INSERT INTO dbo.ExportManifest
(BatchNumber, PageNumber, PageIndex, S3Key, Success, RowsExported, ErrorMessage, CreatedAt, LoggedAt)
VALUES
(@BatchNumber, @PageNumber, @PageIndex, @S3Key, @Success, @RowsExported, @ErrorMessage, @CreatedAt, @LoggedAt);