--Create the database

CREATE DATABASE IF NOT EXISTS users;
       
--Swap to the created database
       use users;
           
    
--Create The user table:
    CREATE TABLE `users` (
                         `id` bigint unsigned NOT NULL AUTO_INCREMENT,
                         `first_name` varchar(255) NOT NULL,
                         `last_name` varchar(255) NOT NULL,
                         `email` varchar(255) NOT NULL,
                         `phone_number` varchar(20) DEFAULT NULL,
                         `birthday` date DEFAULT NULL,
                         `hashed_password` varchar(255) NOT NULL,
                         `salt` varchar(255) DEFAULT NULL,
                         `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
                         `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                         PRIMARY KEY (`id`),
                         UNIQUE KEY `id` (`id`),
                         UNIQUE KEY `email` (`email`)
                        ) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--Create Custom User Id Table:

    CREATE TABLE `customusersidmodels` (
                                       `userId` varchar(255) NOT NULL,
                                       `email` varchar(255) NOT NULL,
                                       PRIMARY KEY (`userId`),
                                       UNIQUE KEY `email` (`email`)
                                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--Create Roles Model table:
CREATE TABLE `rolesmodels` (
                               `userId` varchar(255) NOT NULL,
                               `Admin` bit(1) NOT NULL,
                               `normalUser` bit(1) NOT NULL,
                               PRIMARY KEY (`userId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--Create timemanagement table:
CREATE TABLE `timemanagementmodels` (
                                        `userId` varchar(255) NOT NULL,
                                        `dayBeingCaptured` datetime NOT NULL,
                                        `totalHours` datetime NOT NULL,
                                        PRIMARY KEY (`userId`)
                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
