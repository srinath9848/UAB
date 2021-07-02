create FUNCTION [dbo].[fnSplit] (
      @InputString                  VARCHAR(max),
      @Delimiter                    VARCHAR(10)
)

RETURNS @Values TABLE (Value VARCHAR(max))

AS
BEGIN

	IF(LEN(@InputString) < 0 OR @InputString IS NULL)
		RETURN

      IF @Delimiter = ' '
      BEGIN
            SET @Delimiter = ','
            SET @InputString = REPLACE(@InputString, ' ', @Delimiter)
      END

      IF (@Delimiter IS NULL OR @Delimiter = '')
            SET @Delimiter = ','

      DECLARE @Value           VARCHAR(max)
      DECLARE @ItemList       VARCHAR(max)
      DECLARE @DelimIndex     INT

      SET @ItemList = @InputString
      SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0)
      WHILE (@DelimIndex != 0)
      BEGIN
            SET @Value = SUBSTRING(@ItemList, 0, @DelimIndex)
            INSERT INTO @Values VALUES (@Value)

            -- Set @ItemList = @ItemList minus one less item
            SET @ItemList = SUBSTRING(@ItemList, @DelimIndex+1, LEN(@ItemList)-@DelimIndex)
            SET @DelimIndex = CHARINDEX(@Delimiter, @ItemList, 0)
      END -- End WHILE

      IF @Value IS NOT NULL -- At least one delimiter was encountered in @InputString
      BEGIN
            SET @Value = @ItemList
            INSERT INTO @Values VALUES (@Value)
      END

      -- No delimiters were encountered in @InputString, so just return @InputString
      ELSE INSERT INTO @Values VALUES (@InputString)

      RETURN

END -- End Function
