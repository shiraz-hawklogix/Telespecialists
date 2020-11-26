Create PROCEDURE [dbo].[UspGetBBICCIData]
AS
BEGIN
select Physician_Id,Physician_Name,Phy_Bci_Value as BCI,Physician_CCI  as CCI
from 
CCIReport_Data  cci join BCI_ReportData bbi on
cci.Physician_Id = bbi.Phy_Id
end
