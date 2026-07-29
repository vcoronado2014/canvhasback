-- 1. Seleccionamos tu base de datos
USE sportcourt; -- Asegúrate de que este sea el nombre exacto de tu esquema

-- 2. Insertamos el SuperAdmin
-- Email: admin@canchas.cl
-- Password: Canchas2026!
-- Rol: 0 (SuperAdmin)
INSERT INTO Users (Email, PasswordHash, Rol, ClubId)
VALUES (
    'admin@canchas.cl', 
    '$2a$11$7y8S.N9b2RjL1WpE5uT4zOqA8vC3mK6nJ7hL9iO0pQ4eR5tY6uI7o', 
    0, 
    NULL
);