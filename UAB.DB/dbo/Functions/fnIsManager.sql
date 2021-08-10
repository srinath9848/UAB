CREATE function fnIsManager(@ProjectId INT,@UserId INT)
RETURNS BIT
BEGIN
	
	DECLARE @IsManager BIT

	IF EXISTS(SELECT 1 FROM projectuser WHERE projectid = @ProjectId AND RoleID IN(4,5) AND userid = @UserId)
	SET @IsManager = 1
	ELSE
	SET @IsManager = 0

	RETURN @IsManager

END