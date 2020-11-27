
-- Author:  <Muhammad Bilal>                      
-- Create date: <08/27/2020>                      
-- Description: <Get the facility list with less than 3 doctors scedualed on prove start date and end date>                      
-- =============================================                      
                    
CREATE procedure [dbo].[sp_flag_facilitites]                    
--DECLARE @start_date decimal set @start_date =   20202440700                 
--DECLARE @end_date decimal set @end_date =    20202440800         
          
@start_date bigint,                 
@end_date bigint               
          
AS                    
BEGIN                    
IF OBJECT_ID(N'tempdb..#tempSch') IS NOT NULL                    
                                BEGIN                    
                                DROP TABLE #tempSch                    
                                END 
								select * into #tempSch from view_user_schedule              
                                Where (TIMEFROM <= @start_date AND TIMETO >= @end_date)                                                      

								--select * from [view_facility_physician]
                                select distinct fac_name as facility_name,count(t.uss_user_id) as physcian_count                    
                                from #tempSch t                                                                 
                                RIGHT JOIN view_facility_physician fp on fp.fap_user_key = t.uss_user_id                                              
                                GROUP BY fac_name                    
                                HAVING count(t.uss_user_id) < 3                    
END          
          
