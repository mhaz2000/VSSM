@echo off
@echo This cmd file creates a Data API Builder configuration based on the chosen database objects.
@echo To run the cmd, create an .env file with the following contents:
@echo dab-connection-string=your connection string
@echo ** Make sure to exclude the .env file from source control **
@echo **
dotnet tool install -g Microsoft.DataApiBuilder
dab init -c dab-config.json --database-type mssql --connection-string "@env('dab-connection-string')" --host-mode Development
@echo Adding tables
dab add "AppLog" --source "[dbo].[AppLogs]" --fields.include "Id,Message,Level,TimeStamp,Exception,IpAddress,UserName,Token,Host,Path,Scheme,Method,Protocol,Query,RequestHeaders,Body,StatusCode" --permissions "anonymous:*" 
dab add "Sync" --source "[dbo].[sync]" --fields.include "ID,tableName,refID,clientID,serverID,date,sync,edit" --permissions "anonymous:*" 
dab add "TblAdjustmentArrivalLine" --source "[dbo].[tblAdjustmentArrivalLines]" --fields.include "AdjustmentArrivalLineID,ArrivalID,ItemID,Quantity,UnitPrice,TotalPrice,UseID,ManufacturerID,DosePerVialID,NumVials,StoreID,ItemDetailID,FirstTimeSource,CommertialNameID,UnitVolume,VVMTypeID,VVMStageID,TotalVolume,BatchNum,Note,PISNumber,TempDosePerVial,ExpiryDate,EntryDate,ProductionDate,IsConfim,IsAccepted,AdjustreasonID,AdjustNote,IsRejected,ResponseDate,IsPrepared,SendDate" --permissions "anonymous:*" 
dab add "TblAdjustment" --source "[dbo].[tblAdjustments]" --fields.include "AdjustmentID,AdjustmentreasonID,FiscalPeriodID,ArrivalLineID,DispatchLineID,ItemDetailStoreID,OldQuantity,NewQuantity,EnteredByID,EntryDate,Date,Note" --permissions "anonymous:*" 
dab add "TblAdjustreason" --source "[dbo].[tblAdjustreasons]" --fields.include "AdjustreasonID,Code,FullName,EntryDate,IsActive,UsedForDispatchAdjust,UserForArrivalAdjust,UsedForStockAdjust,IsWaste,ApplicationForStockSubtraction,ApplicationForStockAddition" --permissions "anonymous:*" 
dab add "TblArrivalLine" --source "[dbo].[tblArrivalLines]" --fields.include "ArrivalLineID,ArrivalID,ItemID,Quantity,UnitPrice,TotalPrice,UseID,ManufacturerID,DosePerVialID,NumVials,StoreID,ItemDetailID,FirstTimeSource,CommertialNameID,UnitVolume,VVMTypeID,VVMStageID,TotalVolume,BatchNum,Note,PISNumber,TempDosePerVial,ExpiryDate,EntryDate,ProductionDate,PackingTypeID,PackageSize,OrderLineID" --permissions "anonymous:*" 
dab add "TblArrival" --source "[dbo].[tblArrivals]" --fields.include "ArrivalID,Date,TotalPrice,ReportNumber,SourceID,FiscalPeriodID,FrieghtCost,EntryDate,IsBeginPeriod,EnteredByID,IsConfirmed,IsCancelled,IsDrafted,IsFirstTime,ConfirmationTIme,IsFirstTimeArrangedAsked,FirstTimeArrangementItemID,FirstTimeArrangementStoreID,PackedVolume,TotalWeight,TotalVolume,SenderID,ReceiverID,DispatchID,Note,frzindc,frzindc_nok,BiddingNumber,OrderID,OrderNumber" --permissions "anonymous:*" 
dab add "TblAuthenticationMode" --source "[dbo].[tblAuthenticationMode]" --fields.include "AuthenticationModeID,AuthenticationModeName,LDAP_Path,Domain" --permissions "anonymous:*" 
dab add "TblBundleDetail" --source "[dbo].[tblBundleDetails]" --fields.include "BundleDetailID,BundleID,ItemID,Quantity,Dose,QuantityPerVial,EntryDate" --permissions "anonymous:*" 
dab add "TblBundle" --source "[dbo].[tblBundles]" --fields.include "BundleID,Code,FullName,ItemID,DosePerVialID,RecipientID,UseID,EntryDate,IsActive,ReceiverID" --permissions "anonymous:*" 
dab add "TblClimate" --source "[dbo].[tblClimate]" --fields.include "ClimateID,Code,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblCommercialName" --source "[dbo].[tblCommercialNames]" --fields.include "CommercialNameID,FullName,Code,IsActive,EntryDate" --permissions "anonymous:*" 
dab add "TblCulture" --source "[dbo].[tblCultures]" --fields.include "Abr,Title,CssFile,isActive,Direction,decsep" --permissions "anonymous:*" 
dab add "TblDispatch" --source "[dbo].[tblDispatches]" --fields.include "DispatchID,SenderID,ReceiverID,FiscalPeriodID,TransportModeID,EnteredByID,ParentDispatch,TotalVolume,AirwaryWill_Reference,RequestNum,DispatchVoucherNumber,Note,IsEndPeriod,IsConfirmed,IsCancelled,DispatchVoucherPrinted,IsDrafted,ConfirmationTime,Date,EntryDate,ReceiverStatus,CancelReason,ArrivalID,TotalPrice,IsDel,frzindc,frzindc_nok,ReceivingModeID,ReportNumber" --permissions "anonymous:*" 
dab add "TblDispatchLine" --source "[dbo].[tblDispatchLines]" --fields.include "DispatchLineID,DispatchID,ItemID,Quantity,UnitPrice,NumVial,DosePerVialID,TotalVolume,TotalPrice,StoreID,UseID,VVMTypeID,VVMStageID,ManufacturerID,ItemDetailStoreID,Note,BatchNumber,ExpiryDate,EntryDate,BundleID" --permissions "anonymous:*" 
dab add "TblDispatchPack" --source "[dbo].[tblDispatchPacks]" --fields.include "DispatchPackID,DispatchID,PackID,EntryDate,NumberOfPack,TotalWeight,TotalVolume,IsPacked,ItemType" --permissions "anonymous:*" 
dab add "TblDosePerVial" --source "[dbo].[tblDosePerVials]" --fields.include "DosePerVialID,NumDosePerVial,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblFiscalPeriod" --source "[dbo].[tblFiscalPeriods]" --fields.include "FiscalPeriodID,StartDate,EndTime,FullName,SuggestedEndDate,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblGlobalConfiguration" --source "[dbo].[tblGlobalConfigurations]" --fields.include "ConfigurationID,Country,Currency,Logo,EntryDate,DefaultWarnDays,offlinesystem,password,Abr,ChainLevel,multigender,EnableVacc,EnableOrder,DateFormat" --permissions "anonymous:*" 
dab add "TblItemDetail" --source "[dbo].[tblItemDetails]" --fields.include "ItemDetailID,FiscalPeriodID,ItemID,DosePerVialID,ManufacturerID,CommercialNameID,VVMTypeID,SourceID,BatchNumber,ExpiryDate,UnitPrice,ProductionDate,UnitVolume,EntryDate,IsConfirmed,PackingTypeID,PackageSize,BiddingNumber" --permissions "anonymous:*" 
dab add "TblItemDetailStore" --source "[dbo].[tblItemDetailStores]" --fields.include "ItemDetailStoreID,ItemDetailID,StoreID,CurrentStock,EntryDate,VVMStageID" --permissions "anonymous:*" 
dab add "TblItemDoseAssignment" --source "[dbo].[tblItemDoseAssignments]" --fields.include "ItemDoseAssignmentID,ItemID,DoseID,VialPerDoseCm3,EntryDate" --permissions "anonymous:*" 
dab add "TblItem" --source "[dbo].[tblItems]" --fields.include "ItemID,ItemClassID,Code,FullName,Volume,UserAboveZeroStore,UserBelowZeroStore,UseOrdinaryStore,DayToWarmExpire,RequiresDiluent,DiluentID,IsActive,EntryDate,OSVOL,UnitPrice,uniCode,OrderID,PresentationID" --permissions "anonymous:*" 
dab add "TblItemsClass" --source "[dbo].[tblItemsClass]" --fields.include "ItemClassID,Code,FullName,EntryDate,Orders,IsActive,VolumeType,Req_Manufacturer,Req_BatchNo,Req_ExpiryDate,Req_DosePerVial,IsUserEditable,Req_VVMType" --permissions "anonymous:*" 
dab add "TblManItemClass" --source "[dbo].[tblManItemClass]" --fields.include "Man_ItemClassID,ItemClassID,ManufacturerID" --permissions "anonymous:*" 
dab add "TblManufacturer" --source "[dbo].[tblManufacturers]" --fields.include "ManufacturerID,Code,FullName,EntryDate,ShowForVac,ShowForOther,IsActive" --permissions "anonymous:*" 
dab add "TblMinMaxItem" --source "[dbo].[tblMinMaxItem]" --fields.include "MinMaxItemID,ItemID,RecipientID,StockMin,StockMax,Min,Max,AnnualNeeds,MaxCalculateOption,NumberShipment,PercentSafety,LeadConsumption,TotalPop,PercentUnder,NumDose,Wastage,PercentCoverage,EntryDate,OrderLevel" --permissions "anonymous:*" 
dab add "TblOrder" --source "[dbo].[tblOrder]" --fields.include "OrderID,SenderID,OrderNumber,SourceID,IsDraft,IsConfirm,IsCancel,FiscalPeriodID,submitDate" --permissions "anonymous:*" 
dab add "TblOrderLine" --source "[dbo].[tblOrderLine]" --fields.include "OrderLineID,OrderID,ItemID,Quantity,UnitPrice,TotalPrice,DosePerVialID,NumVials,RecpID,EstimatedDate,ManufactureID,finished" --permissions "anonymous:*" 
dab add "TblOrderReceive" --source "[dbo].[tblOrderReceive]" --fields.include "OrderRecvID,OrderLineID,RecvCount,RecvdDate,SubmitDate,ExpiryDate,RecpID,BatchNum,StoreID" --permissions "anonymous:*" 
dab add "TblOwnership" --source "[dbo].[tblOwnership]" --fields.include "OwnershipID,Code,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblPackingType" --source "[dbo].[tblPackingType]" --fields.include "PackingTypeID,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblPackItemClass" --source "[dbo].[tblPackItemClass]" --fields.include "PackItemClassID,PackID,ItemClassID" --permissions "anonymous:*" 
dab add "TblPack" --source "[dbo].[tblPacks]" --fields.include "PackID,Code,FullName,NetVolume,ExternalSize,PackWeight,IsActive,EntryDate,UseForVacDL,UseForOS" --permissions "anonymous:*" 
dab add "TblPowerSource" --source "[dbo].[tblPowerSource]" --fields.include "PowerSourceID,Code,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblReceivingMode" --source "[dbo].[tblReceivingModes]" --fields.include "ReceivingModeID,Code,FullName,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblRecipientConfiguration" --source "[dbo].[tblRecipientConfigurations]" --fields.include "RecipientConfigurationID,UseBundles,FreezeQtyDispatchedDl,CurrentLang,EntryDate,CurrentVoucherNumber,RecipientID,LastLogin" --permissions "anonymous:*" 
dab add "TblRecipientMaxItem" --source "[dbo].[tblRecipientMaxItems]" --fields.include "RecipientMaxItemID,ItemID,RecipientID,NumberDose,EntryDate,ReceiverID,RelatedToItem" --permissions "anonymous:*" 
dab add "TblRecipient" --source "[dbo].[tblRecipients]" --fields.include "RecipientID,Code,FullName,EntryDate,IsActive,TransportModeID,Distance,Population,Year,Note,ParentID,StartUp,ChainLevel,ReceivingModeID,Address1,Address2,PostalCode,City,Province,Phone,Email,Facility,LeadTime,ClimateID,OwnershipID,PowerSourceID,TypeOfRoadID,HasLandLine,HasInternet,DistanceTime" --permissions "anonymous:*" 
dab add "TblRequest" --source "[dbo].[tblRequest]" --fields.include "id,SenderID,SendDate,status,ReceiverID,EnteredBy,Note,ConfirmDate,RequestNumber,FiscalPeriodID" --permissions "anonymous:*" 
dab add "TblRequestLine" --source "[dbo].[tblRequestLine]" --fields.include "id,RequestID,ItemID,ItemClassID,anncount,arivcount,usedcount,currentcount,forperiod,offercount,reqcount,Note,confirmcount,DosePerVialID" --permissions "anonymous:*" 
dab add "dboTblRequestLine" --source "[dbo].[tblRequestLines]" --fields.include "RequestLineID,RequestID,ItemID,RequestedQuantity,EntryDate,Note,ConfirmedQuantity" --permissions "anonymous:*" 
dab add "dboTblRequest" --source "[dbo].[tblRequests]" --fields.include "RequestID,SenderID,SendDate,EntryDate,IsConfirmed,IsDraft,ReceiverID,FiscalPeriodID,EnteredBy,ReceiverStatus,Note,IsCanceled" --permissions "anonymous:*" 
dab add "TblRoadType" --source "[dbo].[tblRoadType]" --fields.include "RoadTypeID,Code,FullName,IsActive" --permissions "anonymous:*" 
dab add "TblSource" --source "[dbo].[tblSources]" --fields.include "SourceID,Code,FullName,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblStockHistory" --source "[dbo].[tblStockHistories]" --fields.include "StockHistoryID,ItemID,Month,Year,EndOfMonthStock,EntryDate,RecipientID" --permissions "anonymous:*" 
dab add "TblStore" --source "[dbo].[tblStores]" --fields.include "StoreID,Code,Name,TypeID,FreeVolume,Volume,EntryDate,Note,IsActive,RecipientID,GrossVol,Year,Model,Manufacturer,PQSNo,CoolingUnitNo,RefrigerantGas,RoomBox,PoweSource,Source,ReasonForUnilize,Barcode,PeriodNotUse,UnitCost" --permissions "anonymous:*" 
dab add "TblStoreType" --source "[dbo].[tblStoreTypes]" --fields.include "StoreTypeID,Code,FullName,EntryDate,IsActive,IsUserEditable" --permissions "anonymous:*" 
dab add "TblTransportMode" --source "[dbo].[tblTransportModes]" --fields.include "TransportModeID,Code,FullName,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblUser" --source "[dbo].[tblUsers]" --fields.include "UserID,UserName,Password,FirstName,LastName,Email,RecipientID,IsDeleted,PermissionLevel,EntryDate,EnteredByID,phone1,phone2,storewarrning,IDNumber,CodeEdu,CodeJob,FreeNote,isAdmin,Course,CourseDate,lastlogin" --permissions "anonymous:*" 
dab add "TblUse" --source "[dbo].[tblUses]" --fields.include "UseID,Code,FullName,EntryDate,IsRoutine,IsActive" --permissions "anonymous:*" 
dab add "TblVoucherConfig" --source "[dbo].[tblVoucherConfig]" --fields.include "VoucherConfigurationID,ShowExpiryDate,ShowNumberOfVials,ShowDoseVial,ShowVVMType,ShowVVMStage,ShowStore,ShowUnitPrice,ShowTotalPriceLine,ShowNotesLine,ShowNotes,ShowTotalWeight,ShowPacksNumber,ShowTotalPrice,ShowTotalExternalSize,ShowTotalVolume,FooterNotes,SignPlaceHolder1,SignPlaceHolder2,SignPlaceHolder3,SignPlaceHolder4,ShowItem,ShowBatchNumber,ShowRequestNumber,ShowTransportMode,ShowManufacturer,ShowQuantity,ShowItemCode,ShowPresentation,ShowPackingType,ShowPackageSize,ShowStageDelivery,ShowIndicator,showSource" --permissions "anonymous:*" 
dab add "TblVvmstage" --source "[dbo].[tblVVMStages]" --fields.include "VVMStageID,Code,FullName,EntryDate,IsActive" --permissions "anonymous:*" 
dab add "TblVvmtype" --source "[dbo].[tblVVMTypes]" --fields.include "VVMTypeID,Code,FullName,CanUserChange,EntryDate,IsActive" --permissions "anonymous:*" 
@echo Adding views and tables without primary key
@echo No primary key found for table/view 'syncBasic', using Id column (ID) as key field
dab add "SyncBasicView" --source "[dbo].[syncBasic]" --fields.include "ID,tableName,serverID,date,action" --source.type "view" --source.key-fields "ID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'syncNew', using Id column (ID) as key field
dab add "SyncNewView" --source "[dbo].[syncNew]" --fields.include "ID,tableName,refID,clientID,serverID,date,action" --source.type "view" --source.key-fields "ID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblAdjUserPermision', using first Id column (userid) as key field
dab add "TblAdjUserPermisionView" --source "[dbo].[tblAdjUserPermision]" --fields.include "userid,arr,dis,curinc,curdec,req,ordr,frcst,arv,ship,recp,move" --source.type "view" --source.key-fields "userid" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblAdjustmentArrivals', using first Id column (AdjustmentArrivalID) as key field
dab add "TblAdjustmentArrivalView" --source "[dbo].[tblAdjustmentArrivals]" --fields.include "AdjustmentArrivalID,Date,TotalPrice,ReportNumber,SourceID,FiscalPeriodID,FrieghtCost,EntryDate,IsBeginPeriod,EnteredByID,IsConfirmed,IsCancelled,IsDrafted,IsFirstTime,ConfirmationTIme,IsFirstTimeArrangedAsked,FirstTimeArrangementItemID,FirstTimeArrangementStoreID,PackedVolume,TotalWeight,TotalVolume,SenderID,ReceiverID,DispatchID,IsConfrimForAdjust,AdjustNote" --source.type "view" --source.key-fields "AdjustmentArrivalID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblArrivalRepConfig', using first column (reportNumber) as key field
dab add "TblArrivalRepConfigView" --source "[dbo].[tblArrivalRepConfig]" --fields.include "reportNumber,source,orderPurchaseNumber,itemClass,itemCode,itemName,Manufacturer,BatchNumber,DosePerVial,NumberOfVial,Quantity,unitPrice,totalPrice,activity,expireDate,unitVolume,totalVolume,arrivalDate" --source.type "view" --source.key-fields "reportNumber" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblCountryCode', using first column (code) as key field
dab add "TblCountryCodeView" --source "[dbo].[tblCountryCode]" --fields.include "code,country" --source.type "view" --source.key-fields "code" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblDispatchRepConfig', using first column (dispatchDate) as key field
dab add "TblDispatchRepConfigView" --source "[dbo].[tblDispatchRepConfig]" --fields.include "dispatchDate,recipient,requestNumber,itemCode,itemClass,itemName,batchNumber,voucherNumber,quantity,dosePerVial,numberOfVial,expDate,store,activity" --source.type "view" --source.key-fields "dispatchDate" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblEduType', using first Id column (EduTypeID) as key field
dab add "TblEduTypeView" --source "[dbo].[tblEduType]" --fields.include "EduTypeID,FullName,IsActive" --source.type "view" --source.key-fields "EduTypeID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblHelp', using Id column (id) as key field
dab add "TblHelpView" --source "[dbo].[tblHelp]" --fields.include "id,title,pid,field1,field2,field3,field4,title1,title2,title3,title4,field5,field6,title5,title6,orderby" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblItemsCode', using first column (code) as key field
dab add "TblItemsCodeView" --source "[dbo].[tblItemsCode]" --fields.include "code,UnicCode,ItemClass" --source.type "view" --source.key-fields "code" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblJobType', using first Id column (JobTypeID) as key field
dab add "TblJobTypeView" --source "[dbo].[tblJobType]" --fields.include "JobTypeID,FullName,IsActive" --source.type "view" --source.key-fields "JobTypeID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblLevels', using first Id column (LevelID) as key field
dab add "TblLevelView" --source "[dbo].[tblLevels]" --fields.include "LevelID,ShipCount,SafetyStockPercentage,SafetyStockMonth,Note,MaxDay" --source.type "view" --source.key-fields "LevelID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblMessages', using first Id column (messageID) as key field
dab add "TblMessageView" --source "[dbo].[tblMessages]" --fields.include "messageID,message,sender,reciever,isnew,ondate,Enterby,subject" --source.type "view" --source.key-fields "messageID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblNews', using first Id column (newsID) as key field
dab add "TblNewsView" --source "[dbo].[tblNews]" --fields.include "newsID,title,description,enable,enterbyID,ondate" --source.type "view" --source.key-fields "newsID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblPresentationType', using first Id column (PresentationTypeID) as key field
dab add "TblPresentationTypeView" --source "[dbo].[tblPresentationType]" --fields.include "PresentationTypeID,FullName,IsActive" --source.type "view" --source.key-fields "PresentationTypeID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblReasonForNotutiliz', using first Id column (ReasonForNotutilizID) as key field
dab add "TblReasonForNotutilizView" --source "[dbo].[tblReasonForNotutiliz]" --fields.include "ReasonForNotutilizID,Code,FullName,IsActive" --source.type "view" --source.key-fields "ReasonForNotutilizID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblRequestsNew', using first Id column (RequestID) as key field
dab add "TblRequestsNewView" --source "[dbo].[tblRequestsNew]" --fields.include "RequestID,ItemID,itemclass,levelhigher,annualrequiement,Note,Quantityreci,Quantitydisp,Currentstock,QuantitySug,QuantityRequ,dateFR,IsConfirm,IsCanceled" --source.type "view" --source.key-fields "RequestID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblStockLog', using first Id column (StockLogID) as key field
dab add "TblStockLogView" --source "[dbo].[tblStockLog]" --fields.include "StockLogID,EntryDate,ItemDetailStoreID,BatchNumber,FromTo,Arrival,Dispatch,Balance,Remark,vochrep" --source.type "view" --source.key-fields "StockLogID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblStoreUsageWarning', using first column (dry) as key field
dab add "TblStoreUsageWarningView" --source "[dbo].[tblStoreUsageWarning]" --fields.include "dry,bz,az" --source.type "view" --source.key-fields "dry" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblUserHome', using first column (message) as key field
dab add "TblUserHomeView" --source "[dbo].[tblUserHome]" --fields.include "message,loginmessage,message2,message3" --source.type "view" --source.key-fields "message" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblUserLog', using Id column (id) as key field
dab add "TblUserLogView" --source "[dbo].[tblUserLog]" --fields.include "id,userID,userActivityId,logDate" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Info_AgeGroup', using first Id column (AgeGroupID) as key field
dab add "TblVaccInfoAgeGroupView" --source "[dbo].[tblVacc_Info_AgeGroup]" --fields.include "AgeGroupID,Title,Enable,OrderID" --source.type "view" --source.key-fields "AgeGroupID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Info_DoseCat', using first Id column (DoseCatID) as key field
dab add "TblVaccInfoDoseCatView" --source "[dbo].[tblVacc_Info_DoseCat]" --fields.include "DoseCatID,Title,Enable,ItemID" --source.type "view" --source.key-fields "DoseCatID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Info_TargetGroup', using first Id column (TargetID) as key field
dab add "TblVaccInfoTargetGroupView" --source "[dbo].[tblVacc_Info_TargetGroup]" --fields.include "TargetID,Title,Enable,OrderID" --source.type "view" --source.key-fields "TargetID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Item_AgeGroup', using first Id column (ItemID) as key field
dab add "TblVaccItemAgeGroupView" --source "[dbo].[tblVacc_Item_AgeGroup]" --fields.include "ItemID,AgeGroupID" --source.type "view" --source.key-fields "ItemID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Item_DoseCat', using first Id column (ItemID) as key field
dab add "TblVaccItemDoseCatView" --source "[dbo].[tblVacc_Item_DoseCat]" --fields.include "ItemID,DoseCatID" --source.type "view" --source.key-fields "ItemID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Item_TargetGroup', using first Id column (ItemID) as key field
dab add "TblVaccItemTargetGroupView" --source "[dbo].[tblVacc_Item_TargetGroup]" --fields.include "ItemID,TargetGroupID" --source.type "view" --source.key-fields "ItemID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Submit', using first Id column (VaccSubmitID) as key field
dab add "TblVaccSubmitView" --source "[dbo].[tblVacc_Submit]" --fields.include "VaccSubmitID,RecpID,Year,Month,SubmitDate,IsConfirm" --source.type "view" --source.key-fields "VaccSubmitID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'tblVacc_Submit_Detail', using first Id column (DetailID) as key field
dab add "TblVaccSubmitDetailView" --source "[dbo].[tblVacc_Submit_Detail]" --fields.include "DetailID,SubmitID,ItemID,TargetID,AgeID,DoseID,Male,Femail,Total" --source.type "view" --source.key-fields "DetailID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'userSync', using Id column (ID) as key field
dab add "UserSyncView" --source "[dbo].[userSync]" --fields.include "ID,syncBasicID,recpID" --source.type "view" --source.key-fields "ID" --permissions "anonymous:*" 
@echo No primary key found for table/view 'userSync2', using Id column (ID) as key field
dab add "UserSync2View" --source "[dbo].[userSync2]" --fields.include "ID,syncID,recpID" --source.type "view" --source.key-fields "ID" --permissions "anonymous:*" 
@echo Adding relationships
dab update TblAdjustmentArrivalLine --relationship TblAdjustreason --target.entity TblAdjustreason --cardinality one
dab update TblAdjustreason --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblArrival --target.entity TblArrival --cardinality one
dab update TblArrival --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblCommercialName --target.entity TblCommercialName --cardinality one
dab update TblCommercialName --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblDosePerVial --target.entity TblDosePerVial --cardinality one
dab update TblDosePerVial --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblItemDetail --target.entity TblItemDetail --cardinality one
dab update TblItemDetail --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblManufacturer --target.entity TblManufacturer --cardinality one
dab update TblManufacturer --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblStore --target.entity TblStore --cardinality one
dab update TblStore --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblUse --target.entity TblUse --cardinality one
dab update TblUse --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblVvmstage --target.entity TblVvmstage --cardinality one
dab update TblVvmstage --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustmentArrivalLine --relationship TblVvmtype --target.entity TblVvmtype --cardinality one
dab update TblVvmtype --relationship TblAdjustmentArrivalLine --target.entity TblAdjustmentArrivalLine --cardinality many
dab update TblAdjustment --relationship TblItemDetailStore --target.entity TblItemDetailStore --cardinality one
dab update TblItemDetailStore --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblAdjustment --relationship TblAdjustreason --target.entity TblAdjustreason --cardinality one
dab update TblAdjustreason --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblAdjustment --relationship TblArrivalLine --target.entity TblArrivalLine --cardinality one
dab update TblArrivalLine --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblAdjustment --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality one
dab update TblDispatchLine --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblAdjustment --relationship TblFiscalPeriod --target.entity TblFiscalPeriod --cardinality one
dab update TblFiscalPeriod --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblAdjustment --relationship TblUser --target.entity TblUser --cardinality one
dab update TblUser --relationship TblAdjustment --target.entity TblAdjustment --cardinality many
dab update TblArrivalLine --relationship TblArrival --target.entity TblArrival --cardinality one
dab update TblArrival --relationship TblArrivalLine --target.entity TblArrivalLine --cardinality many
dab update TblArrival --relationship TblFiscalPeriod --target.entity TblFiscalPeriod --cardinality one
dab update TblFiscalPeriod --relationship TblArrival --target.entity TblArrival --cardinality many
dab update TblArrival --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblArrival --target.entity TblArrival --cardinality many
dab update TblArrival --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblArrival --target.entity TblArrival --cardinality many
dab update TblArrival --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblArrival --target.entity TblArrival --cardinality many
dab update TblArrival --relationship TblSource --target.entity TblSource --cardinality one
dab update TblSource --relationship TblArrival --target.entity TblArrival --cardinality many
dab update TblBundleDetail --relationship TblBundle --target.entity TblBundle --cardinality one
dab update TblBundle --relationship TblBundleDetail --target.entity TblBundleDetail --cardinality many
dab update TblDispatch --relationship TblArrival --target.entity TblArrival --cardinality one
dab update TblArrival --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblFiscalPeriod --target.entity TblFiscalPeriod --cardinality one
dab update TblFiscalPeriod --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblReceivingMode --target.entity TblReceivingMode --cardinality one
dab update TblReceivingMode --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblTransportMode --target.entity TblTransportMode --cardinality one
dab update TblTransportMode --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatch --relationship TblUser --target.entity TblUser --cardinality one
dab update TblUser --relationship TblDispatch --target.entity TblDispatch --cardinality many
dab update TblDispatchLine --relationship TblBundle --target.entity TblBundle --cardinality one
dab update TblBundle --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblDispatch --target.entity TblDispatch --cardinality one
dab update TblDispatch --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblDosePerVial --target.entity TblDosePerVial --cardinality one
dab update TblDosePerVial --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblItemDetailStore --target.entity TblItemDetailStore --cardinality one
dab update TblItemDetailStore --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblManufacturer --target.entity TblManufacturer --cardinality one
dab update TblManufacturer --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblStore --target.entity TblStore --cardinality one
dab update TblStore --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblUse --target.entity TblUse --cardinality one
dab update TblUse --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblVvmstage --target.entity TblVvmstage --cardinality one
dab update TblVvmstage --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblDispatchLine --relationship TblVvmtype --target.entity TblVvmtype --cardinality one
dab update TblVvmtype --relationship TblDispatchLine --target.entity TblDispatchLine --cardinality many
dab update TblItemDetail --relationship TblCommercialName --target.entity TblCommercialName --cardinality one
dab update TblCommercialName --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblDosePerVial --target.entity TblDosePerVial --cardinality one
dab update TblDosePerVial --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblFiscalPeriod --target.entity TblFiscalPeriod --cardinality one
dab update TblFiscalPeriod --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblManufacturer --target.entity TblManufacturer --cardinality one
dab update TblManufacturer --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblPackingType --target.entity TblPackingType --cardinality one
dab update TblPackingType --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblSource --target.entity TblSource --cardinality one
dab update TblSource --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetail --relationship TblVvmtype --target.entity TblVvmtype --cardinality one
dab update TblVvmtype --relationship TblItemDetail --target.entity TblItemDetail --cardinality many
dab update TblItemDetailStore --relationship TblItemDetail --target.entity TblItemDetail --cardinality one
dab update TblItemDetail --relationship TblItemDetailStore --target.entity TblItemDetailStore --cardinality many
dab update TblItemDetailStore --relationship TblStore --target.entity TblStore --cardinality one
dab update TblStore --relationship TblItemDetailStore --target.entity TblItemDetailStore --cardinality many
dab update TblItemDetailStore --relationship TblVvmstage --target.entity TblVvmstage --cardinality one
dab update TblVvmstage --relationship TblItemDetailStore --target.entity TblItemDetailStore --cardinality many
dab update TblItemDoseAssignment --relationship TblDosePerVial --target.entity TblDosePerVial --cardinality one
dab update TblDosePerVial --relationship TblItemDoseAssignment --target.entity TblItemDoseAssignment --cardinality many
dab update TblItemDoseAssignment --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblItemDoseAssignment --target.entity TblItemDoseAssignment --cardinality many
dab update TblItem --relationship TblItemsClass --target.entity TblItemsClass --cardinality one
dab update TblItemsClass --relationship TblItem --target.entity TblItem --cardinality many
dab update TblManItemClass --relationship TblItemsClass --target.entity TblItemsClass --cardinality one
dab update TblItemsClass --relationship TblManItemClass --target.entity TblManItemClass --cardinality many
dab update TblManItemClass --relationship TblManufacturer --target.entity TblManufacturer --cardinality one
dab update TblManufacturer --relationship TblManItemClass --target.entity TblManItemClass --cardinality many
dab update TblMinMaxItem --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblMinMaxItem --target.entity TblMinMaxItem --cardinality many
dab update TblMinMaxItem --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblMinMaxItem --target.entity TblMinMaxItem --cardinality many
dab update TblRecipientMaxItem --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblRecipientMaxItem --target.entity TblRecipientMaxItem --cardinality many
dab update TblRecipientMaxItem --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblRecipientMaxItem --target.entity TblRecipientMaxItem --cardinality many
dab update TblRecipientMaxItem --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblRecipientMaxItem --target.entity TblRecipientMaxItem --cardinality many
dab update TblRecipient --relationship TblClimate --target.entity TblClimate --cardinality one
dab update TblClimate --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRecipient --relationship TblOwnership --target.entity TblOwnership --cardinality one
dab update TblOwnership --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRecipient --relationship TblPowerSource --target.entity TblPowerSource --cardinality one
dab update TblPowerSource --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRecipient --relationship TblReceivingMode --target.entity TblReceivingMode --cardinality one
dab update TblReceivingMode --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRecipient --relationship TblRoadType --target.entity TblRoadType --cardinality one
dab update TblRoadType --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRecipient --relationship TblTransportMode --target.entity TblTransportMode --cardinality one
dab update TblTransportMode --relationship TblRecipient --target.entity TblRecipient --cardinality many
dab update TblRequest --relationship TblFiscalPeriod --target.entity TblFiscalPeriod --cardinality one
dab update TblFiscalPeriod --relationship TblRequest --target.entity TblRequest --cardinality many
dab update TblRequest --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblRequest --target.entity TblRequest --cardinality many
dab update TblRequest --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblRequest --target.entity TblRequest --cardinality many
dab update TblRequest --relationship TblUser --target.entity TblUser --cardinality one
dab update TblUser --relationship TblRequest --target.entity TblRequest --cardinality many
dab update TblRequestLine --relationship TblDosePerVial --target.entity TblDosePerVial --cardinality one
dab update TblDosePerVial --relationship TblRequestLine --target.entity TblRequestLine --cardinality many
dab update TblRequestLine --relationship TblItem --target.entity TblItem --cardinality one
dab update TblItem --relationship TblRequestLine --target.entity TblRequestLine --cardinality many
dab update TblRequestLine --relationship TblRequest --target.entity TblRequest --cardinality one
dab update TblRequest --relationship TblRequestLine --target.entity TblRequestLine --cardinality many
dab update TblRequestLine --relationship TblRequest --target.entity TblRequest --cardinality one
dab update TblRequest --relationship TblRequestLine --target.entity TblRequestLine --cardinality many
dab update TblStore --relationship TblRecipient --target.entity TblRecipient --cardinality one
dab update TblRecipient --relationship TblStore --target.entity TblStore --cardinality many
dab update TblStore --relationship TblStoreType --target.entity TblStoreType --cardinality one
dab update TblStoreType --relationship TblStore --target.entity TblStore --cardinality many
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
