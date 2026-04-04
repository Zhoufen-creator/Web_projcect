USE Web_Project_Db;
GO
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ------------------------------------------------------------
    -- 0. KIỂM TRA USER GỐC DO DbInitializer TẠO
    ------------------------------------------------------------
    DECLARE @AdminBaseId NVARCHAR(450)    = (SELECT TOP 1 Id FROM Users WHERE Email = 'admin@gmail.com');
    DECLARE @DoctorBaseId NVARCHAR(450)   = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor@gmail.com');
    DECLARE @EmployeeBaseId NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'employee@gmail.com');
    DECLARE @PatientBaseId NVARCHAR(450)  = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient@gmail.com');

    IF @AdminBaseId IS NULL OR @DoctorBaseId IS NULL OR @EmployeeBaseId IS NULL OR @PatientBaseId IS NULL
    BEGIN
        PRINT N'Chưa có đủ 4 tài khoản gốc. Hãy chạy web 1 lần trước để DbInitializer tạo user.';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    PRINT N'Đã tìm thấy 4 tài khoản gốc.';

    ------------------------------------------------------------
    -- 1. LẤY PASSWORD HASH MẪU TỪ USER GỐC
    ------------------------------------------------------------
    DECLARE @DoctorPasswordHash NVARCHAR(MAX)   = (SELECT TOP 1 PasswordHash FROM Users WHERE Email = 'doctor@gmail.com');
    DECLARE @PatientPasswordHash NVARCHAR(MAX)  = (SELECT TOP 1 PasswordHash FROM Users WHERE Email = 'patient@gmail.com');
    DECLARE @EmployeePasswordHash NVARCHAR(MAX) = (SELECT TOP 1 PasswordHash FROM Users WHERE Email = 'employee@gmail.com');
    DECLARE @AdminPasswordHash NVARCHAR(MAX)    = (SELECT TOP 1 PasswordHash FROM Users WHERE Email = 'admin@gmail.com');

    DECLARE @RoleAdminId NVARCHAR(450)    = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin');
    DECLARE @RoleDoctorId NVARCHAR(450)   = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Doctor');
    DECLARE @RoleEmployeeId NVARCHAR(450) = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Employee');
    DECLARE @RolePatientId NVARCHAR(450)  = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Patient');

    IF @RoleAdminId IS NULL OR @RoleDoctorId IS NULL OR @RoleEmployeeId IS NULL OR @RolePatientId IS NULL
    BEGIN
        PRINT N'Chưa có đủ role. Hãy kiểm tra DbInitializer.';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    ------------------------------------------------------------
    -- 2. TẠO THÊM DOCTOR USERS
    --    Password chung: Doctor@123
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'doctor1@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'BS Nguyễn Văn An', '1985-03-10', N'Nam', N'Quận 5, TP.HCM', 0, GETDATE(),
            'doctor1@gmail.com', 'DOCTOR1@GMAIL.COM',
            'doctor1@gmail.com', 'DOCTOR1@GMAIL.COM', 1,
            @DoctorPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0901000001', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'doctor2@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'BS Trần Thị Bình', '1988-07-21', N'Nữ', N'Quận 10, TP.HCM', 0, GETDATE(),
            'doctor2@gmail.com', 'DOCTOR2@GMAIL.COM',
            'doctor2@gmail.com', 'DOCTOR2@GMAIL.COM', 1,
            @DoctorPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0901000002', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'doctor3@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'BS Lê Hoàng Minh', '1983-11-15', N'Nam', N'Quận 3, TP.HCM', 0, GETDATE(),
            'doctor3@gmail.com', 'DOCTOR3@GMAIL.COM',
            'doctor3@gmail.com', 'DOCTOR3@GMAIL.COM', 1,
            @DoctorPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0901000003', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'doctor4@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'BS Phạm Thu Hà', '1990-02-09', N'Nữ', N'Bình Thạnh, TP.HCM', 0, GETDATE(),
            'doctor4@gmail.com', 'DOCTOR4@GMAIL.COM',
            'doctor4@gmail.com', 'DOCTOR4@GMAIL.COM', 1,
            @DoctorPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0901000004', 0,
            0, NULL, 1, 0
        );
    END

    ------------------------------------------------------------
    -- 3. TẠO THÊM EMPLOYEE USERS
    --    Password chung: Employee@123
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'employee1@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Nguyễn Thị Mai', '1996-04-11', N'Nữ', N'Quận 8, TP.HCM', 0, GETDATE(),
            'employee1@gmail.com', 'EMPLOYEE1@GMAIL.COM',
            'employee1@gmail.com', 'EMPLOYEE1@GMAIL.COM', 1,
            @EmployeePasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0912000001', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'employee2@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Lê Quốc Huy', '1995-12-01', N'Nam', N'Tân Bình, TP.HCM', 0, GETDATE(),
            'employee2@gmail.com', 'EMPLOYEE2@GMAIL.COM',
            'employee2@gmail.com', 'EMPLOYEE2@GMAIL.COM', 1,
            @EmployeePasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0912000002', 0,
            0, NULL, 1, 0
        );
    END

    ------------------------------------------------------------
    -- 4. TẠO THÊM PATIENT USERS
    --    Password chung: Patient@123
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'patient1@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Phạm Ngọc Anh', '2005-10-30', N'Nữ', N'Quận 12, TP.HCM', 0, GETDATE(),
            'patient1@gmail.com', 'PATIENT1@GMAIL.COM',
            'patient1@gmail.com', 'PATIENT1@GMAIL.COM', 1,
            @PatientPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0933000001', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'patient2@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Nguyễn Văn Hòa', '1999-06-12', N'Nam', N'Quận Gò Vấp, TP.HCM', 0, GETDATE(),
            'patient2@gmail.com', 'PATIENT2@GMAIL.COM',
            'patient2@gmail.com', 'PATIENT2@GMAIL.COM', 1,
            @PatientPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0933000002', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'patient3@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Trần Thị Lan', '2001-08-08', N'Nữ', N'Quận 6, TP.HCM', 0, GETDATE(),
            'patient3@gmail.com', 'PATIENT3@GMAIL.COM',
            'patient3@gmail.com', 'PATIENT3@GMAIL.COM', 1,
            @PatientPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0933000003', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'patient4@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Lê Minh Tâm', '1997-01-19', N'Nam', N'Quận 7, TP.HCM', 0, GETDATE(),
            'patient4@gmail.com', 'PATIENT4@GMAIL.COM',
            'patient4@gmail.com', 'PATIENT4@GMAIL.COM', 1,
            @PatientPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0933000004', 0,
            0, NULL, 1, 0
        );
    END

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'patient5@gmail.com')
    BEGIN
        INSERT INTO Users
        (
            Id, Name, DateOfBirth, Gender, Address, IsSpam, CreateAt,
            UserName, NormalizedUserName,
            Email, NormalizedEmail, EmailConfirmed,
            PasswordHash, SecurityStamp, ConcurrencyStamp,
            PhoneNumber, PhoneNumberConfirmed,
            TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
        )
        VALUES
        (
            CONVERT(NVARCHAR(450), NEWID()), N'Võ Thu Uyên', '2003-03-25', N'Nữ', N'Thủ Đức, TP.HCM', 0, GETDATE(),
            'patient5@gmail.com', 'PATIENT5@GMAIL.COM',
            'patient5@gmail.com', 'PATIENT5@GMAIL.COM', 1,
            @PatientPasswordHash, CONVERT(NVARCHAR(100), NEWID()), CONVERT(NVARCHAR(100), NEWID()),
            '0933000005', 0,
            0, NULL, 1, 0
        );
    END

    ------------------------------------------------------------
    -- 5. GÁN ROLE CHO CÁC USER MỚI
    ------------------------------------------------------------
    INSERT INTO UserRoles (UserId, RoleId)
    SELECT u.Id, @RoleDoctorId
    FROM Users u
    WHERE u.Email IN ('doctor1@gmail.com', 'doctor2@gmail.com', 'doctor3@gmail.com', 'doctor4@gmail.com')
    AND NOT EXISTS (
        SELECT 1 FROM UserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = @RoleDoctorId
    );

    INSERT INTO UserRoles (UserId, RoleId)
    SELECT u.Id, @RoleEmployeeId
    FROM Users u
    WHERE u.Email IN ('employee1@gmail.com', 'employee2@gmail.com')
    AND NOT EXISTS (
        SELECT 1 FROM UserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = @RoleEmployeeId
    );

    INSERT INTO UserRoles (UserId, RoleId)
    SELECT u.Id, @RolePatientId
    FROM Users u
    WHERE u.Email IN ('patient1@gmail.com', 'patient2@gmail.com', 'patient3@gmail.com', 'patient4@gmail.com', 'patient5@gmail.com')
    AND NOT EXISTS (
        SELECT 1 FROM UserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = @RolePatientId
    );

    ------------------------------------------------------------
    -- 6. THÊM CHUYÊN KHOA NẾU CHƯA CÓ
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Specialties WHERE Name = N'Nội tổng quát')
    BEGIN
        INSERT INTO Specialties (Name, AveragePatientLoad, MaxPatientsPerWeek)
        VALUES (N'Nội tổng quát', 12, 100);
    END

    IF NOT EXISTS (SELECT 1 FROM Specialties WHERE Name = N'Tai mũi họng')
    BEGIN
        INSERT INTO Specialties (Name, AveragePatientLoad, MaxPatientsPerWeek)
        VALUES (N'Tai mũi họng', 10, 90);
    END

    IF NOT EXISTS (SELECT 1 FROM Specialties WHERE Name = N'Tim mạch')
    BEGIN
        INSERT INTO Specialties (Name, AveragePatientLoad, MaxPatientsPerWeek)
        VALUES (N'Tim mạch', 8, 80);
    END

    IF NOT EXISTS (SELECT 1 FROM Specialties WHERE Name = N'Da liễu')
    BEGIN
        INSERT INTO Specialties (Name, AveragePatientLoad, MaxPatientsPerWeek)
        VALUES (N'Da liễu', 9, 85);
    END

    ------------------------------------------------------------
    -- 6. TẠO HỒ SƠ DOCTOR
    ------------------------------------------------------------
    DECLARE @DoctorUser1 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor1@gmail.com');
    DECLARE @DoctorUser2 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor2@gmail.com');
    DECLARE @DoctorUser3 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor3@gmail.com');
    DECLARE @DoctorUser4 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor4@gmail.com');

    IF @DoctorUser1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Doctors WHERE UserId = @DoctorUser1)
    BEGIN
        INSERT INTO Doctors (SpecialtyId, LicenseNumber, Qualifications, UserId)
        VALUES ((SELECT TOP 1 Id FROM Specialties WHERE Name = N'Nội tổng quát'), N'BS101', N'Bác sĩ chuyên khoa I Nội tổng quát', @DoctorUser1);
    END

    IF @DoctorUser2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Doctors WHERE UserId = @DoctorUser2)
    BEGIN
        INSERT INTO Doctors (SpecialtyId, LicenseNumber, Qualifications, UserId)
        VALUES ((SELECT TOP 1 Id FROM Specialties WHERE Name = N'Tai mũi họng'), N'BS102', N'Bác sĩ chuyên khoa I Tai mũi họng', @DoctorUser2);
    END

    IF @DoctorUser3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Doctors WHERE UserId = @DoctorUser3)
    BEGIN
        INSERT INTO Doctors (SpecialtyId, LicenseNumber, Qualifications, UserId)
        VALUES ((SELECT TOP 1 Id FROM Specialties WHERE Name = N'Tim mạch'), N'BS103', N'Bác sĩ chuyên khoa I Tim mạch', @DoctorUser3);
    END

    IF @DoctorUser4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Doctors WHERE UserId = @DoctorUser4)
    BEGIN
        INSERT INTO Doctors (SpecialtyId, LicenseNumber, Qualifications, UserId)
        VALUES ((SELECT TOP 1 Id FROM Specialties WHERE Name = N'Da liễu'), N'BS104', N'Bác sĩ chuyên khoa I Da liễu', @DoctorUser4);
    END

    ------------------------------------------------------------
    -- 7. TẠO HỒ SƠ EMPLOYEE
    ------------------------------------------------------------
    DECLARE @EmployeeUser1 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'employee1@gmail.com');
    DECLARE @EmployeeUser2 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'employee2@gmail.com');

    IF @EmployeeUser1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Employees WHERE UserId = @EmployeeUser1)
    BEGIN
        INSERT INTO Employees (Position, Department, UserId)
        VALUES (N'Nhân viên tiếp nhận', N'Lễ tân', @EmployeeUser1);
    END

    IF @EmployeeUser2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Employees WHERE UserId = @EmployeeUser2)
    BEGIN
        INSERT INTO Employees (Position, Department, UserId)
        VALUES (N'Nhân viên điều phối', N'Hành chính', @EmployeeUser2);
    END

    ------------------------------------------------------------
    -- 8. TẠO HỒ SƠ PATIENT
    ------------------------------------------------------------
    DECLARE @PatientUser1 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient1@gmail.com');
    DECLARE @PatientUser2 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient2@gmail.com');
    DECLARE @PatientUser3 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient3@gmail.com');
    DECLARE @PatientUser4 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient4@gmail.com');
    DECLARE @PatientUser5 NVARCHAR(450) = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient5@gmail.com');

    IF @PatientUser1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Patients WHERE UserId = @PatientUser1)
    BEGIN
        INSERT INTO Patients (BloodType, Height, Weight, HealthInsuranceNumber, MedicalHistory, Allergies, UserId)
        VALUES (N'O+', 160, 50, N'BHYT101', N'Viêm dạ dày nhẹ', N'Hải sản', @PatientUser1);
    END

    IF @PatientUser2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Patients WHERE UserId = @PatientUser2)
    BEGIN
        INSERT INTO Patients (BloodType, Height, Weight, HealthInsuranceNumber, MedicalHistory, Allergies, UserId)
        VALUES (N'A+', 170, 63, N'BHYT102', N'Viêm xoang theo mùa', N'Penicillin', @PatientUser2);
    END

    IF @PatientUser3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Patients WHERE UserId = @PatientUser3)
    BEGIN
        INSERT INTO Patients (BloodType, Height, Weight, HealthInsuranceNumber, MedicalHistory, Allergies, UserId)
        VALUES (N'B+', 158, 48, N'BHYT103', N'Dị ứng da nhẹ', N'Không', @PatientUser3);
    END

    IF @PatientUser4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Patients WHERE UserId = @PatientUser4)
    BEGIN
        INSERT INTO Patients (BloodType, Height, Weight, HealthInsuranceNumber, MedicalHistory, Allergies, UserId)
        VALUES (N'AB+', 175, 70, N'BHYT104', N'Huyết áp cao nhẹ', N'Tôm cua', @PatientUser4);
    END

    IF @PatientUser5 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Patients WHERE UserId = @PatientUser5)
    BEGIN
        INSERT INTO Patients (BloodType, Height, Weight, HealthInsuranceNumber, MedicalHistory, Allergies, UserId)
        VALUES (N'O-', 162, 54, N'BHYT105', N'Viêm họng tái phát', N'Không', @PatientUser5);
    END

    ------------------------------------------------------------
    -- 9. TẠO THUỐC MẪU
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name = N'Paracetamol 500mg')
    BEGIN
        INSERT INTO Medicines (Name, Description, Price)
        VALUES (N'Paracetamol 500mg', N'Hạ sốt, giảm đau', 15000);
    END

    IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name = N'Amoxicillin 500mg')
    BEGIN
        INSERT INTO Medicines (Name, Description, Price)
        VALUES (N'Amoxicillin 500mg', N'Kháng sinh điều trị nhiễm khuẩn', 35000);
    END

    IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name = N'Cetirizine 10mg')
    BEGIN
        INSERT INTO Medicines (Name, Description, Price)
        VALUES (N'Cetirizine 10mg', N'Thuốc chống dị ứng', 18000);
    END

    IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name = N'Omeprazole 20mg')
    BEGIN
        INSERT INTO Medicines (Name, Description, Price)
        VALUES (N'Omeprazole 20mg', N'Hỗ trợ dạ dày', 22000);
    END

    ------------------------------------------------------------
    -- 10. TẠO DỊCH VỤ Y TẾ MẪU
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM MedicalServices WHERE Name = N'Khám tổng quát')
    BEGIN
        INSERT INTO MedicalServices (Name, Description, Price)
        VALUES (N'Khám tổng quát', N'Khám lâm sàng tổng quát', 100000);
    END

    IF NOT EXISTS (SELECT 1 FROM MedicalServices WHERE Name = N'Nội soi tai mũi họng')
    BEGIN
        INSERT INTO MedicalServices (Name, Description, Price)
        VALUES (N'Nội soi tai mũi họng', N'Kiểm tra tai mũi họng chuyên sâu', 250000);
    END

    IF NOT EXISTS (SELECT 1 FROM MedicalServices WHERE Name = N'Đo điện tim')
    BEGIN
        INSERT INTO MedicalServices (Name, Description, Price)
        VALUES (N'Đo điện tim', N'Đánh giá hoạt động tim mạch', 180000);
    END

    ------------------------------------------------------------
    -- 11. LẤY ID NGHIỆP VỤ
    ------------------------------------------------------------
    DECLARE @DoctorA INT = (SELECT TOP 1 Id FROM Doctors WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor@gmail.com'));
    DECLARE @DoctorB INT = (SELECT TOP 1 Id FROM Doctors WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor1@gmail.com'));
    DECLARE @DoctorC INT = (SELECT TOP 1 Id FROM Doctors WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor2@gmail.com'));
    DECLARE @DoctorD INT = (SELECT TOP 1 Id FROM Doctors WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor3@gmail.com'));
    DECLARE @DoctorE INT = (SELECT TOP 1 Id FROM Doctors WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'doctor4@gmail.com'));

    DECLARE @PatientA INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient@gmail.com'));
    DECLARE @PatientB INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient1@gmail.com'));
    DECLARE @PatientC INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient2@gmail.com'));
    DECLARE @PatientD INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient3@gmail.com'));
    DECLARE @PatientE INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient4@gmail.com'));
    DECLARE @PatientF INT = (SELECT TOP 1 Id FROM Patients WHERE UserId = (SELECT TOP 1 Id FROM Users WHERE Email = 'patient5@gmail.com'));

    ------------------------------------------------------------
    -- 12. TẠO CA TRỰC CHO BÁC SĨ
    ------------------------------------------------------------
    IF @DoctorA IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DoctorSchedules WHERE DoctorId = @DoctorA)
    BEGIN
        INSERT INTO DoctorSchedules (StartTime, EndTime, MaxPatient, DoctorId)
        VALUES
        (DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 11, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 8, @DoctorA),
        (DATEADD(HOUR, 13, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 17, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 8, @DoctorA);
    END

    IF @DoctorB IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DoctorSchedules WHERE DoctorId = @DoctorB)
    BEGIN
        INSERT INTO DoctorSchedules (StartTime, EndTime, MaxPatient, DoctorId)
        VALUES
        (DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 11, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 8, @DoctorB),
        (DATEADD(HOUR, 13, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 17, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 8, @DoctorB);
    END

    IF @DoctorC IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DoctorSchedules WHERE DoctorId = @DoctorC)
    BEGIN
        INSERT INTO DoctorSchedules (StartTime, EndTime, MaxPatient, DoctorId)
        VALUES
        (DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 11, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 6, @DoctorC),
        (DATEADD(HOUR, 13, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 17, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 6, @DoctorC);
    END

    IF @DoctorD IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DoctorSchedules WHERE DoctorId = @DoctorD)
    BEGIN
        INSERT INTO DoctorSchedules (StartTime, EndTime, MaxPatient, DoctorId)
        VALUES
        (DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 11, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 5, @DoctorD),
        (DATEADD(HOUR, 13, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 17, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 5, @DoctorD);
    END

    IF @DoctorE IS NOT NULL AND NOT EXISTS (SELECT 1 FROM DoctorSchedules WHERE DoctorId = @DoctorE)
    BEGIN
        INSERT INTO DoctorSchedules (StartTime, EndTime, MaxPatient, DoctorId)
        VALUES
        (DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 11, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 5, @DoctorE),
        (DATEADD(HOUR, 13, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), DATEADD(HOUR, 17, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 5, @DoctorE);
    END

    ------------------------------------------------------------
    -- 13. TẠO NHIỀU APPOINTMENTS MẪU
    -- Status lưu dạng chuỗi vì DbContext đang HasConversion<string>()
    ------------------------------------------------------------
    IF @DoctorA IS NOT NULL AND @DoctorB IS NOT NULL AND @DoctorC IS NOT NULL AND @DoctorD IS NOT NULL AND @DoctorE IS NOT NULL
       AND @PatientA IS NOT NULL AND @PatientB IS NOT NULL AND @PatientC IS NOT NULL AND @PatientD IS NOT NULL AND @PatientE IS NOT NULL AND @PatientF IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Appointments WHERE ReasonForVisit = N'Sốt ho đau họng kéo dài')
    BEGIN
        INSERT INTO Appointments
        (
            CreatedAt, ScheduledDate, Status, ReasonForVisit,
            IsCheckedIn, CheckinTime, CancellationTime, CancellationReason,
            IsReminder24hSent, Reminder24hSentAt, IsReminder2hSent, Reminder2hSentAt,
            PatientId, DoctorId
        )
        VALUES
        (GETDATE(), DATEADD(HOUR, 8, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Pending',   N'Sốt ho đau họng kéo dài', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientA, @DoctorA),
        (GETDATE(), DATEADD(HOUR, 9, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Confirmed', N'Mệt mỏi đau đầu', 1, GETDATE(), NULL, NULL, 0, NULL, 0, NULL, @PatientB, @DoctorA),
        (GETDATE(), DATEADD(HOUR, 10, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Pending',   N'Đau bụng khó tiêu', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientC, @DoctorB),
        (GETDATE(), DATEADD(HOUR, 14, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Pending',   N'Đau họng nghẹt mũi', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientD, @DoctorC),
        (GETDATE(), DATEADD(HOUR, 15, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Pending',   N'Hồi hộp đau ngực nhẹ', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientE, @DoctorD),
        (GETDATE(), DATEADD(HOUR, 16, CAST(CAST(GETDATE()+1 AS DATE) AS DATETIME)), 'Pending',   N'Ngứa nổi mẩn dị ứng da', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientF, @DoctorE),

        (GETDATE(), DATEADD(DAY, -3, DATEADD(HOUR, 9, CAST(CAST(GETDATE() AS DATE) AS DATETIME))),  'Completed', N'Viêm mũi dị ứng', 1, DATEADD(DAY, -3, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientA, @DoctorC),
        (GETDATE(), DATEADD(DAY, -5, DATEADD(HOUR, 8, CAST(CAST(GETDATE() AS DATE) AS DATETIME))),  'Completed', N'Khám sức khỏe tổng quát', 1, DATEADD(DAY, -5, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientB, @DoctorA),
        (GETDATE(), DATEADD(DAY, -7, DATEADD(HOUR, 10, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Đau họng kéo dài', 1, DATEADD(DAY, -7, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientC, @DoctorC),
        (GETDATE(), DATEADD(DAY, -10, DATEADD(HOUR, 14, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Cancelled', N'Đau bụng nhẹ', 0, NULL, DATEADD(DAY, -10, GETDATE()), N'Bận đột xuất', 0, NULL, 0, NULL, @PatientD, @DoctorB);
    END

    ------------------------------------------------------------
    -- 14. TẠO THÊM DỮ LIỆU ĐỂ TEST "TẢI KHÁM THEO KHOA"
    --     Dồn nhiều lịch vào Tai mũi họng để dashboard thấy tăng tải
    ------------------------------------------------------------
    IF @DoctorC IS NOT NULL
       AND @PatientA IS NOT NULL AND @PatientB IS NOT NULL AND @PatientC IS NOT NULL AND @PatientD IS NOT NULL AND @PatientE IS NOT NULL AND @PatientF IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Appointments WHERE ReasonForVisit = N'Test tải khoa TMH 01')
    BEGIN
        INSERT INTO Appointments
        (
            CreatedAt, ScheduledDate, Status, ReasonForVisit,
            IsCheckedIn, CheckinTime, CancellationTime, CancellationReason,
            IsReminder24hSent, Reminder24hSentAt, IsReminder2hSent, Reminder2hSentAt,
            PatientId, DoctorId
        )
        VALUES
        (DATEADD(DAY, -28, GETDATE()), DATEADD(DAY, -28, DATEADD(HOUR, 9, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Test tải khoa TMH 01', 1, DATEADD(DAY, -28, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientA, @DoctorC),
        (DATEADD(DAY, -21, GETDATE()), DATEADD(DAY, -21, DATEADD(HOUR, 10, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Test tải khoa TMH 02', 1, DATEADD(DAY, -21, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientB, @DoctorC),
        (DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, DATEADD(HOUR, 11, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Test tải khoa TMH 03', 1, DATEADD(DAY, -14, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientC, @DoctorC),

        (DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -2, DATEADD(HOUR, 8, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Test tải khoa TMH 04', 1, DATEADD(DAY, -2, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientD, @DoctorC),
        (DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, DATEADD(HOUR, 9, CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'Completed', N'Test tải khoa TMH 05', 1, DATEADD(DAY, -1, GETDATE()), NULL, NULL, 0, NULL, 0, NULL, @PatientE, @DoctorC),
        (GETDATE(), DATEADD(HOUR, 10, CAST(CAST(GETDATE() AS DATE) AS DATETIME)), 'Pending', N'Test tải khoa TMH 06', 0, NULL, NULL, NULL, 0, NULL, 0, NULL, @PatientF, @DoctorC);
    END

    ------------------------------------------------------------
    -- 15. TẠO MEDICAL EXAMINATIONS CHO CÁC LỊCH COMPLETED
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM MedicalExaminations)
    BEGIN
        INSERT INTO MedicalExaminations
        (
            StartTime, EndTime, Symptoms, Diagnosis, DoctorAvoid, Status,
            AppointmentId, DoctorId, PatientId
        )
        SELECT
            a.ScheduledDate,
            DATEADD(MINUTE, 20 + (a.Id % 10), a.ScheduledDate),
            a.ReasonForVisit,
            CASE
                WHEN s.Name = N'Tai mũi họng' THEN N'Viêm tai mũi họng nhẹ'
                WHEN s.Name = N'Tim mạch' THEN N'Triệu chứng tim mạch mức độ nhẹ'
                WHEN s.Name = N'Da liễu' THEN N'Dị ứng da nhẹ'
                ELSE N'Tình trạng ổn định'
            END,
            N'Uống thuốc đúng giờ, tái khám nếu nặng hơn',
            'Completed',
            a.Id,
            a.DoctorId,
            a.PatientId
        FROM Appointments a
        INNER JOIN Doctors d ON a.DoctorId = d.Id
        LEFT JOIN Specialties s ON d.SpecialtyId = s.Id
        WHERE a.Status = 'Completed';
    END

    ------------------------------------------------------------
    -- 16. TẠO PRESCRIPTIONS MẪU
    ------------------------------------------------------------
    DECLARE @Medicine1 INT = (SELECT TOP 1 Id FROM Medicines WHERE Name = N'Paracetamol 500mg');
    DECLARE @Medicine2 INT = (SELECT TOP 1 Id FROM Medicines WHERE Name = N'Amoxicillin 500mg');
    DECLARE @Medicine3 INT = (SELECT TOP 1 Id FROM Medicines WHERE Name = N'Cetirizine 10mg');

    IF NOT EXISTS (SELECT 1 FROM Prescriptions)
       AND @Medicine1 IS NOT NULL AND @Medicine2 IS NOT NULL AND @Medicine3 IS NOT NULL
       AND EXISTS (SELECT 1 FROM MedicalExaminations)
    BEGIN
        INSERT INTO Prescriptions (Dosage, Quantity, MedicineId, MedicalExaminationId)
        SELECT N'Ngày 2 lần sau ăn', 10, @Medicine1, TOP1.Id
        FROM (SELECT TOP 1 Id FROM MedicalExaminations ORDER BY Id) AS TOP1;

        INSERT INTO Prescriptions (Dosage, Quantity, MedicineId, MedicalExaminationId)
        SELECT N'Ngày 3 lần sau ăn', 14, @Medicine2, TOP1.Id
        FROM (SELECT TOP 1 Id FROM MedicalExaminations ORDER BY Id DESC) AS TOP1;

        INSERT INTO Prescriptions (Dosage, Quantity, MedicineId, MedicalExaminationId)
        SELECT N'Ngày 1 viên buổi tối', 7, @Medicine3, TOP1.Id
        FROM (SELECT TOP 1 Id FROM MedicalExaminations ORDER BY Id) AS TOP1;
    END

    ------------------------------------------------------------
    -- 17. TẠO EXAMINATION SERVICES MẪU
    ------------------------------------------------------------
    DECLARE @Service1 INT = (SELECT TOP 1 Id FROM MedicalServices WHERE Name = N'Khám tổng quát');
    DECLARE @Service2 INT = (SELECT TOP 1 Id FROM MedicalServices WHERE Name = N'Nội soi tai mũi họng');
    DECLARE @Service3 INT = (SELECT TOP 1 Id FROM MedicalServices WHERE Name = N'Đo điện tim');

    IF NOT EXISTS (SELECT 1 FROM ExaminationServices)
       AND @Service1 IS NOT NULL AND @Service2 IS NOT NULL AND @Service3 IS NOT NULL
       AND EXISTS (SELECT 1 FROM Appointments WHERE Status = 'Completed')
       AND EXISTS (SELECT 1 FROM Appointments WHERE Status = 'Pending')
    BEGIN
        INSERT INTO ExaminationServices (Quantity, Result, CompletedAt, MedicalServiceId, AppointmentId)
        SELECT 1, N'Kết quả bình thường', GETDATE(), @Service1, a.Id
        FROM (SELECT TOP 1 Id FROM Appointments WHERE Status = 'Completed' ORDER BY Id) a;

        INSERT INTO ExaminationServices (Quantity, Result, CompletedAt, MedicalServiceId, AppointmentId)
        SELECT 1, N'Niêm mạc họng viêm nhẹ', GETDATE(), @Service2, a.Id
        FROM (SELECT TOP 1 Id FROM Appointments WHERE Status = 'Completed' ORDER BY Id DESC) a;

        INSERT INTO ExaminationServices (Quantity, Result, CompletedAt, MedicalServiceId, AppointmentId)
        SELECT 1, N'Nhịp tim ổn định', GETDATE(), @Service3, a.Id
        FROM (SELECT TOP 1 Id FROM Appointments WHERE Status = 'Pending' ORDER BY Id) a;
    END

    ------------------------------------------------------------
    -- 18. TẠO NOTIFICATIONS MẪU
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Notifications)
       AND @PatientUser1 IS NOT NULL AND @PatientUser2 IS NOT NULL AND @PatientUser3 IS NOT NULL AND @PatientUser4 IS NOT NULL AND @PatientUser5 IS NOT NULL
    BEGIN
        INSERT INTO Notifications (Title, Message, CreatedAt, IsRead, UserId)
        VALUES
        (N'Nhắc lịch khám', N'Bạn có lịch khám vào ngày mai lúc 08:00.', GETDATE(), 0, @PatientUser1),
        (N'Ước lượng thời gian khám', N'Bạn nên đến trước 15 phút để check-in.', GETDATE(), 0, @PatientUser2),
        (N'Lịch khám đã xác nhận', N'Lịch khám của bạn đã được hệ thống xác nhận.', GETDATE(), 0, @PatientUser3),
        (N'Thông báo hệ thống', N'Vui lòng cập nhật hồ sơ bệnh nhân đầy đủ.', GETDATE(), 0, @PatientUser4),
        (N'Nhắc tái khám', N'Bạn nên tái khám sau 7 ngày theo chỉ định bác sĩ.', GETDATE(), 0, @PatientUser5);
    END

    ------------------------------------------------------------
    -- 19. KIỂM TRA KẾT QUẢ
    ------------------------------------------------------------
    SELECT COUNT(*) AS TotalUsers FROM Users;
    SELECT COUNT(*) AS TotalDoctors FROM Doctors;
    SELECT COUNT(*) AS TotalEmployees FROM Employees;
    SELECT COUNT(*) AS TotalPatients FROM Patients;
    SELECT COUNT(*) AS TotalAppointments FROM Appointments;
    SELECT COUNT(*) AS TotalMedicalExaminations FROM MedicalExaminations;
    SELECT COUNT(*) AS TotalNotifications FROM Notifications;

    COMMIT TRANSACTION;
    PRINT N'Đã seed dữ liệu mẫu thành công.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT N'Có lỗi, đã rollback.';
    THROW;
END CATCH;
GO
