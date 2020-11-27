       
CREATE procedure [dbo].[sp_phy_scheduals]          
         
@startDate bigint,          
@endDate bigint,          
@isAdmin bit,  
@isSuperAdmin bit,  
@userId varchar(128),
@SchType varchar(100)
AS          
BEGIN          
          
select distinct          
ush.uss_key as Id,          
ush.uss_user_id as UserId,          
asp.UserInitial as Title,           
(asp.UserInitial + ' - ' + SUBSTRING(Convert(VARCHAR,ush.uss_time_from_calc,8),1,5) + ' to ' + SUBSTRING(CONVERT(VARCHAR,ush.uss_time_to_calc,8),1,5) + '#div#(' + CONVERT(VARCHAR,asp.CredentialIndex) + ')#/div#') as  TitleBig,          
CASE WHEN ush.uss_description = '' THEN '' ELSE ush.uss_description END as [Description],          
ush.uss_date as ScheduleDate,          
ush.uss_time_from_calc as Start,          
ush.uss_time_to_calc as [End],          
ush.uss_is_active as IsActive,          
asp.FirstName + ' ' + asp.LastName as FullName,          
CASE WHEN ((ush.uss_time_to - 864000000000) > 0) THEN CAST(1 as bit) ELSE CAST(0 as bit) END as IsAllDay,          
ush.uss_custome_rate as Rate,          
ush.uss_shift_key as ShiftId,          
Convert(decimal(10,2),asp.CredentialIndex) as [PhyIndexRate],      
ush.uss_is_publish as IsPublish      
from           
aspnetusers asp          
JOIN user_schedule ush on asp.Id = ush.uss_user_id
JOIN AspNetUserRoles aspur on aspur.UserId = asp.Id
JOIN AspNetRoles aspr on aspr.Id = aspur.RoleId
Where IsActive = 1          
AND IsDeleted = 0          
AND (ush.uss_date_num between @startDate AND @endDate)          
AND ((@isAdmin = 1 AND 1=1) OR (@isAdmin = 0 AND ush.uss_user_id = @userId))          
AND ((@isSuperAdmin = 1 AND 1=1) OR (@isSuperAdmin = 0 AND ush.uss_is_publish = 1))  
AND ((@SchType = 'Physician' AND asp.IsStrokeAlert = 1 AND aspr.Name != 'AOC') OR (@SchType = 'aoc' AND aspr.Name = 'AOC'))  
--AND asp.IsStrokeAlert = 1    
order by ScheduleDate          
          
END

