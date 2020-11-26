
create procedure [dbo].[sp_recent_used_diagnosis_code]      
@UserId varchar(128)      
AS      
BEGIN 
IF OBJECT_ID(N'tempdb..#tempCodes') IS NOT NULL BEGIN   DROP TABLE #tempCodes  END  
;WITH icd_codes(icd_code,cas_billing_diagnosis) AS
(
    SELECT
        LEFT(cas_billing_diagnosis, CHARINDEX(';', cas_billing_diagnosis + ';') - 1),
        STUFF(cas_billing_diagnosis, 1, CHARINDEX(';', cas_billing_diagnosis + ';'), '')
    FROM [case]
	  where cas_phy_key = @UserId       
	  and cas_billing_diagnosis is not null      
	  and (cas_created_date >= '2020-10-01 03:32:46.823' OR cas_modified_date >= '2020-10-01 03:32:46.823') 
    UNION all

    SELECT
        LEFT(cas_billing_diagnosis, CHARINDEX(';', cas_billing_diagnosis + ';') - 1),
        STUFF(cas_billing_diagnosis, 1, CHARINDEX(';', cas_billing_diagnosis + ';'), '')
    FROM icd_codes
    WHERE
        cas_billing_diagnosis > ''
)


SELECT top 10
   LTRIM(RTRIM(icd_code)) as icd_code,count(icd_code) as icdCount
   into #tempCodes
FROM icd_codes
group by icd_code
order by 2 desc

select icd_code from #tempCodes
End
