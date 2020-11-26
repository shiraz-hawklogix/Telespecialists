create VIEW [dbo].[view_user_schedule]  
AS  
SELECT        dbo.user_schedule.uss_key, dbo.user_schedule.uss_user_id, dbo.AspNetUsers.IsStrokeAlert, dbo.user_schedule.uss_date_num as DayNumber, 
			dbo.user_schedule.uss_time_from_calc_num as TIMEFROM,dbo.user_schedule.uss_time_to_calc_num as TIMETO  
FROM            dbo.user_schedule INNER JOIN  
                         dbo.AspNetUsers ON dbo.user_schedule.uss_user_id = dbo.AspNetUsers.Id  
WHERE        (dbo.AspNetUsers.IsStrokeAlert = 'true')
