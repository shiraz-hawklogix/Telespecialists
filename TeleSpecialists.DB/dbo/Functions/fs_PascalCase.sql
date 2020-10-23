CREATE FUNCTION [dbo].[fs_PascalCase]
(
    @Text nVarChar(MAX)
)
RETURNS nVarChar(MAX)
AS
BEGIN
        SET @Text = LOWER(@Text)--This step is optional.  Keep if you want the code below to control all casing. - 11/26/2013 - MCR.
    DECLARE @New nVarChar(MAX) = (CASE WHEN @Text IS NULL THEN NULL ELSE '' END)--Still return null when source is null. - 11/26/2013 - MCR.
    DECLARE @Len   Int = LEN(REPLACE(@Text, ' ', '_'))--If you want to count/keep trailing-spaces, you MUST use this!!! - 11/26/2013 - MCR.
    DECLARE @Index Int = 1--Sql-Server is 1-based, not 0-based.
    WHILE (@Index <= @Len)
        IF (SUBSTRING(@Text, @Index, 1) LIKE '[^a-z]' AND @Index + 1 <= @Len)--If not alpha and there are more character(s).
            SELECT @New = @New + UPPER(SUBSTRING(@Text, @Index, 2)), @Index = @Index + 2
        ELSE
            SELECT @New = @New +       SUBSTRING(@Text, @Index, 1) , @Index = @Index + 1

    --If @Text is null, then @Len will be Null, and everything will be null.
    --If @Text is '',   then (@Len - 1) will be -1, so ABS() it to use 1 instead, which will still return ''.
    RETURN ( UPPER(LEFT(@New, 1)) + RIGHT(@New, ABS(@Len - 1)) )
END
