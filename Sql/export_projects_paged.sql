SELECT
    p.ProjectId,
    p.Name,
    JSON_QUERY((
SELECT TaskId, Title, Status, AssignedTo
    FROM dbo.Task t
    WHERE t.ProjectId = p.ProjectId
    FOR JSON PATH
)) AS Tasks,
    JSON_QUERY((
SELECT d.DocumentId, FileName,
        JSON_QUERY((
SELECT CommentId, Author, Message
        FROM dbo.DocumentComment c
        WHERE c.DocumentId = d.DocumentId
        FOR JSON PATH
)) AS Comments
    FROM dbo.Document d
    WHERE d.ProjectId = p.ProjectId
    FOR JSON PATH
)) AS Documents
FROM dbo.Project p
ORDER BY p.ProjectId
OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;
