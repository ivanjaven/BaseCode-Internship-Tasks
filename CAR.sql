/*
SQLyog Ultimate v13.1.1 (64 bit)
MySQL - 10.4.28-MariaDB : Database - DEALERSHIP
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`DEALERSHIP` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci */;

USE `DEALERSHIP`;

/*Table structure for table `CAR` */

DROP TABLE IF EXISTS `CAR`;

CREATE TABLE `CAR` (
  `CAR_ID` int(100) NOT NULL AUTO_INCREMENT,
  `CAR_MODEL` varchar(100) DEFAULT NULL,
  `CAR_BRAND` varchar(100) DEFAULT NULL,
  `CAR_HORSEPOWER` varchar(100) DEFAULT NULL,
  `CAR_SEATER` varchar(100) DEFAULT NULL,
  `CAR_COLOR` varchar(100) DEFAULT NULL,
  `CAR_PRICE` varchar(100) DEFAULT NULL,
  `CAR_STATUS` varchar(1) DEFAULT 'A',
  `CAR_CREATE_DATE` datetime DEFAULT current_timestamp(),
  `CAR_UPDATE_DATE` datetime DEFAULT current_timestamp(),
  PRIMARY KEY (`CAR_ID`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

/*Data for the table `CAR` */

insert  into `CAR`(`CAR_ID`,`CAR_MODEL`,`CAR_BRAND`,`CAR_HORSEPOWER`,`CAR_SEATER`,`CAR_COLOR`,`CAR_PRICE`,`CAR_STATUS`,`CAR_CREATE_DATE`,`CAR_UPDATE_DATE`) values 
(1,'Model S','Tesla','450','5','Blue','79990','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(2,'Model 3','Tesla','358','5','Black','39990','D','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(3,'Civic','Honda','158','5','Blue','21550','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(4,'Accord','Honda','275','4','Red','24870','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(5,'Camry','Toyota','250','5','Silver','24570','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(6,'Corolla','Toyota','139','5','Grey','20020','D','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(7,'Mustang','Ford','450','4','Velvet Red','49770','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(8,'Fiesta','Ford','120','5','Green','15000','A','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(9,'3 Series','BMW','255','5','Black','41050','D','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(10,'X3','BMW','248','5','White','43100','D','2023-06-05 21:16:10','2023-06-05 21:16:10'),
(11,'Model SSR','Tesla','900','2','Matte Black','99999','D','2023-06-06 22:57:46','2023-06-07 12:44:34'),
(12,'Test2','Standard','100','4','Floral Pink','15000','D','2023-06-07 17:53:14','2023-06-09 18:24:30'),
(15,'EcoBoost Z','Ford','450','5','Midnight Blue','55000','D','2023-06-09 18:52:52','2023-06-09 18:54:26');

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
