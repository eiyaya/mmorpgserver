DROP DATABASE IF EXISTS `giftcodedb`;
CREATE DATABASE `giftcodedb`;
DROP TABLE IF EXISTS `giftcodedb`.`giftcode`;
CREATE TABLE `giftcodedb`.`giftcode` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `code` VARCHAR(20) NOT NULL,
    `state` TINYINT(1) NOT NULL DEFAULT '0',
    `createtime` TIMESTAMP NOT NULL DEFAULT NOW(),
	`channelId` INT(11) DEFAULT '0',
    PRIMARY KEY (`id`),
    UNIQUE KEY `code` (`code`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;


DROP TABLE IF EXISTS `giftcodedb`.`question`;
CREATE TABLE `giftcodedb`.`question` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Title` VARCHAR(20) NOT NULL DEFAULT '',
    `Text` VARCHAR(255) NOT NULL DEFAULT '',
	`ById` BIGINT(11) NOT NULL DEFAULT 0,
	`ByName` VARCHAR(20) NOT NULL DEFAULT '',
    `createtime` TIMESTAMP NOT NULL DEFAULT NOW(),
    `state` INT(5) NOT NULL DEFAULT '0',
    PRIMARY KEY (`id`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;

DELIMITER $$

DROP PROCEDURE IF EXISTS `giftcodedb`.`updatestate` $$
CREATE PROCEDURE `giftcodedb`.`updatestate`(
in incode VARCHAR(20),
in channelId INT(11)
)

top:BEGIN
declare r_return int;
  declare t_id int;

  declare exit handler for sqlexception
  begin
    rollback;
    set r_return=1;
    select r_return as myreturn;
  end;

  start transaction;
  SELECT id into t_id from giftcode where code=incode and state=0 and (giftcode.channelId = channelId or giftcode.channelId = 0) for update;
    if isnull(t_id) then
      set r_return=2;
      select r_return as myreturn;
      leave top;
    end if;

  UPDATE giftcode set state=1 where id=t_id;
  set r_return=0;
  select r_return as myreturn;

commit;
END$$

DELIMITER ;


grant all PRIVILEGES on giftcodedb.* to dbuser@'%' identified by 'dbuser';
FLUSH PRIVILEGES;

/*
grant all PRIVILEGES on giftcodedb.* to dbuser@'localhost' identified by 'dbuser';
FLUSH PRIVILEGES;
*/

update mysql.proc set DEFINER='dbuser@%' WHERE NAME='updatestate' AND db='giftcodedb';