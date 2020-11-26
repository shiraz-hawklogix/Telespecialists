CREATE procedure sp_get_busy_physicians_ids      
      
@fac_key varchar(128),      
@CurrentDate datetime      
      
--declare @busy_physcian Table (physician_id varchar(128))      
--declare @fac_key varchar(128) set @fac_key = 'dd762698-e625-4fcc-8841-37e56a0530a9'      
--declare @CurrentDate datetime set @CurrentDate = '10/23/2020 3:00:10 AM'      
AS      
BEGIN      
IF OBJECT_ID(N'tempdb..#UserSchedule') IS NOT NULL BEGIN   DROP TABLE #UserSchedule  END       
IF OBJECT_ID(N'tempdb..#physicians') IS NOT NULL BEGIN   DROP TABLE #physicians  END      
IF OBJECT_ID(N'tempdb..#facility') IS NOT NULL BEGIN   DROP TABLE #facility  END      
IF OBJECT_ID(N'tempdb..#physician_license') IS NOT NULL BEGIN   DROP TABLE #physician_license  END      
IF OBJECT_ID(N'tempdb..#facilityPhysciain') IS NOT NULL BEGIN   DROP TABLE #facilityPhysciain  END      
IF OBJECT_ID(N'tempdb..#physciainLastCasesGroupBY') IS NOT NULL BEGIN   DROP TABLE #physciainLastCasesGroupBY  END      
IF OBJECT_ID(N'tempdb..#physciainLastCases') IS NOT NULL BEGIN   DROP TABLE #physciainLastCases  END      
      
declare @busy_physcian Table (physician_id varchar(128))      
declare @fac_stt_key int;       
      
select uss_user_id into #UserSchedule from user_schedule where @CurrentDate >= uss_time_from_calc AND @CurrentDate <= uss_time_to_calc      
    
      
select asp.Id into #physicians       
from AspNetUsers asp      
join AspNetUserRoles aspur on asp.Id = aspur.UserId      
join AspNetRoles aspr on aspur.RoleId = aspr.Id      
where aspr.Name in ('Physician','Partner Physician')      
AND asp.IsActive = 1      
AND asp.IsDeleted = 0       
AND asp.IsDisable = 0    
        
select * into #facility from facility  where fac_key = @fac_key      
set  @fac_stt_key = (select fac_stt_key from #facility)      
    
      
select distinct phl_user_key into #physician_license from physician_license       
where phl_is_active = 1      
AND @CurrentDate >=  phl_issued_date      
AND ((phl_license_state is null) OR (phl_license_state = @fac_stt_key))      
AND ((phl_expired_date is null) OR (@CurrentDate <=  phl_expired_date))      
    
select distinct fap_user_key into #facilityPhysciain from       
facility_physician fp      
join #physician_license pl on fp.fap_user_key = pl.phl_user_key      
join #physicians phy on fp.fap_user_key = phy.Id      
WHERE fp.fap_fac_key = @fac_key    
AND fp.fap_is_active = 1      
AND fp.fap_is_on_boarded = 1 
AND fp.fap_hide = 0   
AND (1=1 OR fp.fap_user_key in (select uss_user_id from #UserSchedule))      
    
select cas_phy_key,MAX(cas_physician_assign_date) as cas_physician_assign_date into #physciainLastCasesGroupBY from [case]      
where cas_phy_key in (select fap_user_key from #facilityPhysciain)      
AND cas_physician_assign_date IS NOT NULL      
GROUP BY cas_phy_key      
ORDER BY MAX(cas_physician_assign_date) desc      
      
select c.cas_phy_key,c.cas_cst_key,c.cas_physician_assign_date      
into #physciainLastCases      
from [case] c      
join #physciainLastCasesGroupBY plc on c.cas_phy_key = plc.cas_phy_key      
and c.cas_physician_assign_date = plc.cas_physician_assign_date      
and c.cas_phy_key = plc.cas_phy_key      
      
insert into @busy_physcian (physician_id)  select cas_phy_key from #physciainLastCases plc join #physicians p on plc.cas_phy_key = p.Id where plc.cas_cst_key = 18      
      
select * from @busy_physcian  order by 1 asc    
      
END