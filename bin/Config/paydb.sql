DROP DATABASE IF EXISTS `paydb`;
CREATE DATABASE `paydb`;
DROP TABLE IF EXISTS `paydb`.`preorder`;
CREATE TABLE `paydb`.`preorder` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `uid` BIGINT(20) UNSIGNED NOT NULL,
    `playerid` BIGINT(20) UNSIGNED NOT NULL,
    `pac` VARCHAR(50) NOT NULL,
    `orderid` VARCHAR(50) NOT NULL UNIQUE,
    `paytype` INT(10) NOT NULL,
    `amount` FLOAT(5) NOT NULL,
    `createtime` TIMESTAMP NOT NULL ON UPDATE CURRENT_TIMESTAMP,
    `extinfo` VARCHAR(512),
    PRIMARY KEY (`id`),
    KEY `Index_orderid` (`orderid`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;


DROP TABLE IF EXISTS `paydb`.`resultorder`;
CREATE TABLE `paydb`.`resultorder` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `uid` BIGINT(20) UNSIGNED NOT NULL,
    `playerid` BIGINT(20) NOT NULL,
    `pac` VARCHAR(50) NOT NULL,
    `orderid` VARCHAR(50) NOT NULL,
    `paytype` INT(10) NOT NULL,
    `amount` FLOAT(5) NOT NULL,
    `state` TINYINT(3) NOT NULL,
    `bankdatetime` VARCHAR(50) NOT NULL,
    `extinfo` VARCHAR(512),
    PRIMARY KEY (`id`),
    UNIQUE KEY `orderid` (`orderid`),
    KEY `idx_resultorder_state` (`state`)
)  ENGINE=INNODB AUTO_INCREMENT=2 DEFAULT CHARSET=UTF8;

DROP TABLE IF EXISTS `paydb`.`finalorder`;
CREATE TABLE `paydb`.`finalorder` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `uid` BIGINT(20) UNSIGNED NOT NULL,
    `playerid` BIGINT(20) NOT NULL,
    `pac` VARCHAR(50) NOT NULL,
    `orderid` VARCHAR(50) NOT NULL,
    `paytype` INT(10) NOT NULL,
    `amount` FLOAT(5) NOT NULL,
    `state` TINYINT(3) NOT NULL,
    `bankdatetime` VARCHAR(50) NOT NULL,
    `extinfo` VARCHAR(512),
    PRIMARY KEY (`id`),
    UNIQUE KEY `orderid` (`orderid`),
    KEY `idx_finalorder_state` (`state`)
)  ENGINE=INNODB AUTO_INCREMENT=2 DEFAULT CHARSET=UTF8;

DROP TABLE IF EXISTS `paydb`.`errororder`;
CREATE TABLE `paydb`.`errororder` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `uid` BIGINT(20) UNSIGNED NOT NULL,
    `playerid` BIGINT(20) NOT NULL,
    `pac` VARCHAR(50) NOT NULL,
    `orderid` VARCHAR(50) NOT NULL,
    `paytype` INT(10) NOT NULL,
    `amount` FLOAT(5) NOT NULL,
    `state` TINYINT(3) NOT NULL,
    `bankdatetime` VARCHAR(50) NOT NULL,
    `extinfo` VARCHAR(512),
    PRIMARY KEY (`id`),
    UNIQUE KEY `orderid` (`orderid`),
    KEY `idx_errororder_state` (`state`)
)  ENGINE=INNODB AUTO_INCREMENT=2 DEFAULT CHARSET=UTF8;


DELIMITER $$

DROP PROCEDURE IF EXISTS `paydb`.`add_preorder` $$
CREATE PROCEDURE `paydb`.`add_preorder`(
in inuid bigint,
in inplayerid bigint,
in inpac varchar(50),
in inorderid varchar(50),
in inpaytype int(10),
in inamount float,
in inextinfo varchar(512)
)

top:BEGIN
  declare b_return int;
  declare tcount int;

  declare exit handler for sqlexception
  begin
    rollback;
    set b_return=1;
    select b_return as myreturn;
  end;

  SELECT count(*) into tcount FROM preorder WHERE orderid = inorderid for update;
  if tcount != 0 then
    rollback;
    set b_return=3;
    select b_return as myreturn;
    leave top;
  end if;

  INSERT INTO preorder(uid,playerid,pac,orderid,paytype,amount,extinfo)
  values(inuid,inplayerid,inpac,inorderid,inpaytype,inamount,inextinfo);
  set b_return=0;
  select b_return as myreturn;

commit;
END$$

DELIMITER ;

DELIMITER $$
DROP PROCEDURE IF EXISTS `paydb`.`add_resultorder` $$
CREATE PROCEDURE `paydb`.`add_resultorder`(
in inuid bigint,
in inplayerid bigint,
in inpac varchar(50),
in inorderid varchar(50),
in inpaytype int(10),
in inamount varchar(50),
in instate tinyint(3),
in inbankdatetime varchar(50),
in inextinfo varchar(512)
)

top:BEGIN
	declare b_return int;
    declare tcount int;
    
    declare exit handler for sqlexception
    begin
        rollback;
        set b_return=1;
        SELECT b_return AS myreturn;
	end;
    
SELECT SUM(x) INTO tcount FROM
    (SELECT 
        COUNT(id) AS x
    FROM
        errororder
    WHERE
        orderid = inorderid FOR UPDATE UNION ALL SELECT 
        COUNT(id) AS x
    FROM
        finalorder
    WHERE
        orderid = inorderid FOR UPDATE UNION ALL SELECT 
        COUNT(id) AS x
    FROM
        resultorder
    WHERE
        orderid = inorderid FOR UPDATE) aa;
    if tcount != 0 then
    rollback;
    set b_return=3;
SELECT b_return AS myreturn;
    leave top;
    end if;
    
    INSERT INTO resultorder(uid,playerid,pac,orderid,paytype,amount,state,bankdatetime,extinfo)
    values(inuid,inplayerid,inpac,inorderid,inpaytype,inamount,instate,inbankdatetime,inextinfo);
    set b_return=0;
SELECT b_return AS myreturn;
    
commit;
END$$

DELIMITER ;

DELIMITER $$

DROP PROCEDURE IF EXISTS `paydb`.`get_resultorder` $$
CREATE PROCEDURE `paydb`.`get_resultorder`(
)
top:BEGIN 
  declare b_return int;
  declare tid int;

  declare exit handler for sqlexception
  begin
    rollback;
    set b_return=2;
    select b_return as myreturn;
  end;

start transaction;
#  SELECT id into tid from resultorder with(UPDLOCK,READPAST) where state=0 limit 1;
  SELECT id into tid from resultorder where state=0 limit 1 for update;
    if isnull(tid) then
    rollback;
    set b_return=0;
    select b_return as myreturn;
    leave top;
	end if;
    
	set b_return=1;
	SELECT b_return as myreturn;
	SELECT * from resultorder where id=tid;
	UPDATE resultorder set state=1 where id=tid;

commit;
END$$

DELIMITER ;

DROP TABLE IF EXISTS `paydb`.`receipt`;
CREATE TABLE `paydb`.`receipt` (
    `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `characterid` BIGINT(20) UNSIGNED NOT NULL,
	`receiptstring` VARCHAR(255),
	`pac` VARCHAR(50) NOT NULL,
    `state` TINYINT(3) NOT NULL,
    `createtime` TIMESTAMP NOT NULL,
    PRIMARY KEY (`id`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;


DELIMITER $$

DROP PROCEDURE IF EXISTS `paydb`.`add_receipt` $$
CREATE PROCEDURE `paydb`.`add_receipt`(
in incharacterid bigint,
in inreceiptstring varchar(255),
in inpac varchar(50),
in instate tinyint(3)
)

top:BEGIN
  declare b_return int;

  declare exit handler for sqlexception
  begin
    rollback;
    set b_return=1;
    select b_return as myreturn;
  end;

  INSERT INTO receipt(characterid,receiptstring,pac,state)
  values(incharacterid,inreceiptstring,inpac,instate);
  set b_return=0;
  select b_return as myreturn;

commit;
END$$

DELIMITER ;

DELIMITER $$
DROP PROCEDURE IF EXISTS `paydb`.`get_receipt` $$
CREATE PROCEDURE `paydb`.`get_receipt`(
)
top:BEGIN 
  declare b_return int;
  declare tid int;
  declare exit handler for sqlexception
  begin
    rollback;
    set b_return=2;
    select b_return as myreturn;
  end;

start transaction;
  SELECT id into tid from receipt where state=1 and timestampdiff(SECOND,createtime,now()) > 60 limit 1 for update;
    if isnull(tid) then
    SELECT id into tid from receipt where state=0 limit 1 for update;
		if isnull(tid) then
		rollback;
		set b_return=0;
		select b_return as myreturn;
		leave top;
		end if;
    end if;
    
	set b_return=1;
	SELECT b_return as myreturn;
	SELECT * from receipt where id=tid;
	UPDATE receipt set state=1,createtime=now() where id=tid;

commit;
END$$

DELIMITER ;


DELIMITER $$
DROP PROCEDURE IF EXISTS `paydb`.`del_receipt` $$
CREATE PROCEDURE `paydb`.`del_receipt`(
in inid int
)

top:BEGIN
  declare b_return int;

  declare exit handler for sqlexception
  begin
    rollback;
    set b_return=1;
    select b_return as myreturn;
  end;

  delete from receipt where id=inid;
  if row_count()=0 then
		rollback;
		set b_return=2;
		select b_return as myreturn;
		leave top;
      end if;
  set b_return=0;
  select b_return as myreturn;

commit;
END$$
DELIMITER ;



grant all PRIVILEGES on paydb.* to dbuser@'%' identified by 'dbuser';
FLUSH PRIVILEGES;

update mysql.proc set DEFINER='dbuser@%' WHERE NAME='add_preorder' AND db='paydb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='add_resultorder' AND db='paydb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='get_resultorder' AND db='paydb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='add_receipt' AND db='paydb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='get_receipt' AND db='paydb';
update mysql.proc set DEFINER='dbuser@%' WHERE NAME='del_receipt' AND db='paydb';