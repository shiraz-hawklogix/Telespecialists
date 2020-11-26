-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Exec GetPhysicianCaseGridStats '856a7897-2721-4bde-95b5-2378f9587941' 
-- =============================================
CREATE PROCEDURE [dbo].[GetPhysicianCaseGridStats]
	-- Add the parameters for the stored procedure here
	@facilityId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- NOrmal Details
		Select  a.Id, a.[Name],a.status_change_date,a.CredentialIndex, a.phs_name, a.phs_assignment_priority from
		(Select DISTINCT u.Id, (u.FirstName + ' '+ u.LastName) as [Name],u.status_change_date,
		u.CredentialIndex, ps.phs_name, ps.phs_assignment_priority
			From facility_physician f
		join facility fa on fa.fac_key = f.fap_fac_key 
		Join physician_license l on f.fap_user_key =  l.phl_user_key
		Join user_schedule s on s.uss_user_id = l.phl_user_key
		Join AspNetUsers u on u.Id = s.uss_user_id
		join physician_status ps on ps.phs_key = u.status_key
		--Join AspNetUserRoles r on r.UserId = u.Id
		Where --r.RoleId In ('df5b44be-6f9a-4866-9ba8-71c0f44f09d6', '0029737b-f013-4e0b-8a31-1b09524194f9') And
		 l.phl_is_active = 1 And u.IsActive = 1 And f.fap_is_on_boarded = 1
		And f.fap_fac_key = @facilityId
		And (l.phl_license_state is null
			or l.phl_license_state = fa.fac_stt_key)
		And (l.phl_expired_date is null  
			or DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) <= DATEADD(dd, DATEDIFF(dd, 0, l.phl_expired_date), 0) )
		And DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) >= DATEADD(dd, DATEDIFF(dd, 0, l.phl_issued_date), 0)
		And DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) = DATEADD(dd, DATEDIFF(dd, 0, s.uss_date), 0)
		)  a
		Order by  a.phs_assignment_priority
		 , a.CredentialIndex
		 , a.status_change_date

		-- Check individual physician, either busy or not
		IF CURSOR_STATUS('global','cursor_phy')>=-1
		BEGIN
			DEALLOCATE cursor_phy;
		END

		DECLARE 
			@physician_key uniqueidentifier, 
			@row_num   int;
 
		DECLARE cursor_phy CURSOR
		FOR Select  a.Id, ROW_NUMBER() OVER (ORDER BY  Id ) row_num from
			(Select DISTINCT u.Id, (u.FirstName + ' '+ u.LastName) as [Name],u.status_change_date,
				u.CredentialIndex
					From facility_physician f
				join facility fa on fa.fac_key = f.fap_fac_key 
				Join physician_license l on f.fap_user_key =  l.phl_user_key
				Join user_schedule s on s.uss_user_id = l.phl_user_key
				Join AspNetUsers u on u.Id = s.uss_user_id
				--Join AspNetUserRoles r on r.UserId = u.Id
				Where --r.RoleId In ('df5b44be-6f9a-4866-9ba8-71c0f44f09d6', '0029737b-f013-4e0b-8a31-1b09524194f9') And
				 l.phl_is_active = 1 And u.IsActive = 1 And f.fap_is_on_boarded = 1
				And f.fap_fac_key = @facilityId
				And (l.phl_license_state is null
					or l.phl_license_state = fa.fac_stt_key)
				And (l.phl_expired_date is null  
					or DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) <= DATEADD(dd, DATEDIFF(dd, 0, l.phl_expired_date), 0) )
				And DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) >= DATEADD(dd, DATEDIFF(dd, 0, l.phl_issued_date), 0)
				And DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) = DATEADD(dd, DATEDIFF(dd, 0, s.uss_date), 0)
				)  a

		OPEN cursor_phy;
		FETCH NEXT FROM cursor_phy INTO 
			@physician_key, 
			@row_num;
 
		WHILE @@FETCH_STATUS = 0
			BEGIN

				Select top(1) case when c.cas_cst_key = 18 then 'Yes' else 'No' end as IsBusy,
				(u.FirstName + ' '+ u.LastName) as [Physician]
				from dbo.[case] c
				join AspNetUsers u on u.id =  c.cas_phy_key
				where c.cas_phy_key = @physician_key
				order by cas_physician_assign_date desc

				FETCH NEXT FROM cursor_phy INTO 
					@physician_key, 
					@row_num;
			END;
		CLOSE cursor_phy;
END