
DROP DATABASE IF EXISTS `oldplayersdb`;
CREATE DATABASE `oldplayersdb`;
DROP TABLE IF EXISTS `oldplayersdb`.`charreward`;
CREATE TABLE `oldplayersdb`.`charreward` (
    `id` VARCHAR(40) NOT NULL,
    `level` SMALLINT(4) NOT NULL,
    `recharge` BIGINT(20) NOT NULL,
    `vipexp` BIGINT(20) NOT NULL,
    `state` TINYINT(1) NOT NULL DEFAULT '0',
    PRIMARY KEY (`id`)
) ENGINE=INNODB DEFAULT CHARSET=UTF8;

DELIMITER $$

DROP PROCEDURE IF EXISTS `oldplayersdb`.`updatestate` $$
CREATE PROCEDURE `oldplayersdb`.`updatestate`(
    in incode VARCHAR(40)
)

top:BEGIN
  declare r_return int;
  declare t_id varchar(40);

  declare exit handler for sqlexception
  begin
    rollback;
    set r_return=1;
    select r_return as myreturn;
  end;

  start transaction;
  SELECT id into t_id from charreward where id=incode and state=0 for update;
    if isnull(t_id) then
      set r_return=2;
      select r_return as myreturn;
      leave top;
    end if;

  UPDATE charreward set state=1 where id=t_id;
  set r_return=0;
  select r_return as myreturn;

commit;
END$$

DROP PROCEDURE IF EXISTS `oldplayersdb`.`getlevel` $$
CREATE PROCEDURE `oldplayersdb`.`getlevel`(
    in incode VARCHAR(40)
)

top:BEGIN
  declare r_return int;
  declare t_num int;
  declare r_returnRe int;
  declare t_renum int;
  declare r_returnVipexp int;
  declare t_vipnum int;
  declare exit handler for sqlexception
  begin
    rollback;
    set r_return=-1;
    set r_returnRe = -1;
     set r_returnVipexp = -1;
    select r_return as returnLevel;
    select r_returnRe as returnRecharge;
    select r_returnVipexp as returnVipExp;
  end;

  start transaction;
  SELECT level into t_num from charreward where id=incode and state=0;
    if isnull(t_num) then
      set r_return=-2;
      select r_return as returnLevel;
    end if;
 SELECT recharge into t_renum from charreward where id=incode and state=0;
    if isnull(t_renum) then
      set r_returnRe = -2;
      select r_returnRe as returnRecharge;
    end if;
 SELECT vipexp into t_vipnum from charreward where id=incode and state=0;
    if isnull(t_vipnum) then
      set r_returnVipexp = -2;
      select r_returnVipexp as returnVipExp;
    end if;
    
  set r_return=t_num,r_returnRe=t_renum,r_returnVipexp = t_vipnum;
  select r_return as returnLevel,r_returnRe as returnRecharge,r_returnVipexp as returnVipExp;
commit;
END$$

DELIMITER ;

grant all PRIVILEGES on oldplayersdb.* to dbuser@'%' identified by 'dbuser';
FLUSH PRIVILEGES;

update mysql.proc set DEFINER='dbuser@%' WHERE NAME='updatestate' AND db='oldplayersdb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='getlevel' AND db='oldplayersdb';
