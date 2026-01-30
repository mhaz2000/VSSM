@echo off
@echo This cmd file creates a Data API Builder configuration based on the chosen database objects.
@echo To run the cmd, create an .env file with the following contents:
@echo dab-connection-string=your connection string
@echo ** Make sure to exclude the .env file from source control **
@echo **
dotnet tool install -g Microsoft.DataApiBuilder
dab init -c dab-config.json --database-type mssql --connection-string "@env('dab-connection-string')" --host-mode Development
@echo Adding tables
dab add "TblJobType" --source "[dbo].[tblJobType]" --fields.include "JobTypeID,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblUser" --source "[dbo].[tblUsers]" --fields.include "UserID,UserName,Password,FirstName,LastName,Email,RecipientID,IsDeleted,PermissionLevel,EntryDate,EnteredByID,phone1,phone2,storewarrning,IDNumber,CodeEdu,CodeJob,FreeNote,isAdmin,Course,CourseDate,lastlogin" --permissions "anonymous:*" 
@echo Adding views and tables without primary key
@echo Adding relationships
dab update TblUser --relationship TblJobType --target.entity TblJobType --cardinality one
dab update TblJobType --relationship TblUser --target.entity TblUser --cardinality many
dab update TblUser --relationship TblUser --target.entity TblUser --cardinality one
dab update TblUser --relationship TblUser --target.entity TblUser --cardinality many
@echo Adding stored procedures
dab add "GetArrivalAdjLineList" --source "[dbo].[GetArrivalAdjLineList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetItemDetailList" --source "[dbo].[GetItemDetailList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetItemDetailList" --source "[dbo].[-GetItemDetailList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetMsgListBySender" --source "[dbo].[GetMsgListBySender]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetRecepientList" --source "[dbo].[GetRecepientList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetRecepientList" --source "[dbo].[-GetRecepientList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetRequestLineList" --source "[dbo].[GetRequestLineList]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetRequestListByReciever" --source "[dbo].[GetRequestListByReciever]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "GetRequestListBySender" --source "[dbo].[GetRequestListBySender]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SearchHelp" --source "[dbo].[SearchHelp]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
@echo **
@echo ** run 'dab validate' to validate your configuration **
@echo ** run 'dab start' to start the development API host **
