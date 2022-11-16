-- --------------------------------------------------------
-- Sunucu:                       127.0.0.1
-- Sunucu sürümü:                8.0.17 - MySQL Community Server - GPL
-- Sunucu İşletim Sistemi:       Win64
-- HeidiSQL Sürüm:               11.1.0.6116
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- yöntem yapısı dökülüyor nettengelsin.binekDuzeltme
DELIMITER //
CREATE PROCEDURE `binekDuzeltme`()
    DETERMINISTIC
BEGIN
UPDATE ortak SET ortak.category_path=yeniBinekGetir(ortak.category_path);
END//
DELIMITER ;

-- yöntem yapısı dökülüyor nettengelsin.cokluKayitlariTekeDusur
DELIMITER //
CREATE PROCEDURE `cokluKayitlariTekeDusur`(
	IN `firma` VARCHAR(50)
)
BEGIN
	IF firma='motorasin' THEN  
		DELETE t1 FROM motorasin t1 INNER JOIN motorasin t2 WHERE t1.ManufacturerCode=t2.ManufacturerCode AND t1.id<t2.id;
			/*(
				(stockAmountMotorasin(t1.Quantity, t1.MinOrder)<stockAmountMotorasin(t2.Quantity, t2.MinOrder)
				OR
				(
					(stockAmountMotorasin(t1.Quantity, t1.MinOrder)=stockAmountMotorasin(t2.Quantity, t2.MinOrder)
					AND
					(t1.id<t2.id)
				)
			)*/
	ELSEIF firma='dinamik' THEN
		DELETE t1 FROM dinamik t1 INNER JOIN dinamik t2 WHERE	t1.stok_kodu=t2.stok_kodu AND t1.id<t2.id;
	END IF; 
END//
DELIMITER ;

-- yöntem yapısı dökülüyor nettengelsin.degistir
DELIMITER //
CREATE PROCEDURE `degistir`(
	IN `firma` VARCHAR(50)
)
    DETERMINISTIC
BEGIN
	DECLARE n INT DEFAULT 0;
	DECLARE i INT DEFAULT 0;
	
	DECLARE tablo_ VARCHAR(50);
	DECLARE alan_ VARCHAR(50);
	DECLARE eskiDeger_ VARCHAR(50);
	DECLARE yeniDeger_ VARCHAR(50);
	DECLARE sart_ VARCHAR(50);
	
	SELECT COUNT(*) FROM degisim INTO n;
	SET i=0;
	WHILE i<n DO 
		SELECT tabloAdi,alanAdi,eskiDeger,yeniDeger,sart FROM degisim limit i,1 INTO tablo_,alan_,eskiDeger_,yeniDeger_,sart_;
		IF tablo_=firma then 
			IF sart_='r' then
				SET @guncellemeCumlesi=CONCAT('UPDATE ',tablo_,' set ',alan_,'=REGEXP_REPLACE(',alan_,',\'',eskiDeger_,'\',\'',yeniDeger_,'\') WHERE REGEXP_LIKE(',alan_,',\'',eskiDeger_,'\')');
			ELSEIF sart_='c' then
				SET @guncellemeCumlesi=CONCAT('UPDATE ',tablo_,' set ',alan_,'=REPLACE(',alan_,',\'',eskiDeger_,'\',\'',yeniDeger_,'\') WHERE ',alan_,'=\'',eskiDeger_,'\' OR ',alan_,' like \'',eskiDeger_,'>%\'');
			ELSEIF sart_='=' then
				SET @guncellemeCumlesi=CONCAT('UPDATE ',tablo_,' set ',alan_,'=\'',yeniDeger_,'\' WHERE ',alan_,'=\'',eskiDeger_,'\'');
			ELSE 
				SET @guncellemeCumlesi=CONCAT('UPDATE ',tablo_,' set ',alan_,'=REPLACE(',alan_,',\'',eskiDeger_,'\',\'',yeniDeger_,'\') WHERE ',alan_,' LIKE \'%',eskiDeger_,'%\'');
			END IF;
		
		
			PREPARE sss FROM @guncellemeCumlesi;
			EXECUTE sss;
			DEALLOCATE PREPARE sss;
		
		END IF;
		SET i = i + 1;
	END WHILE;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.artmisFiyat
DELIMITER //
CREATE FUNCTION `artmisFiyat`(
	`paraBirimi` VARCHAR(5),
	`fiyat` FLOAT,
	`artis` FLOAT,
	`yuzdeMi` TINYINT,
	`usdKur` FLOAT,
	`eurKur` FLOAT
) RETURNS double
    DETERMINISTIC
BEGIN
if yuzdeMi=1 then
	RETURN ROUND(fiyat+fiyat*artis/100,2);
ELSE
	IF paraBirimi='EUR' then
		RETURN ROUND(fiyat+artis/eurKur,2);
	ELSEIF paraBirimi='USD' then
		RETURN ROUND(fiyat+artis/usdKur,2);
	ELSE
		RETURN ROUND(fiyat+artis,2);
	END if;
END if;
 
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.barkodlar
DELIMITER //
CREATE FUNCTION `barkodlar`(
	`b1` TINYTEXT,
	`b2` TINYTEXT,
	`b3` TINYTEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
	SET b1=REPLACE(b1,'\r','\n');
	SET b2=REPLACE(b2,'\r','\n');
	SET b3=REPLACE(b3,'\r','\n');
	if (INSTR(b1,'\n')>0) then
		SET b1=REGEXP_REPLACE(TRIM(CONCAT(TRIM(SUBSTRING_INDEX(b1,'\n',1)),', ',TRIM(SUBSTRING_INDEX(b1,'\n',-1)))),',$','');
	end if;
	if (INSTR(b2,'\n')>0) then
		SET b2=REGEXP_REPLACE(TRIM(CONCAT(TRIM(SUBSTRING_INDEX(b2,'\n',1)),', ',TRIM(SUBSTRING_INDEX(b2,'\n',-1)))),',$','');
	end if;
	if (INSTR(b3,'\n')>0) then
		SET b3=REGEXP_REPLACE(TRIM(CONCAT(TRIM(SUBSTRING_INDEX(b3,'\n',1)),', ',TRIM(SUBSTRING_INDEX(b3,'\n',-1)))),',$','');
	end if;
	SET b1=TRIM(REPLACE(REPLACE(b1,' ','#'),'\t',''));
	SET b2=TRIM(REPLACE(REPLACE(b2,' ','#'),'\t',''));
	SET b3=TRIM(REPLACE(REPLACE(b3,' ','#'),'\t',''));
	if b2<>'' then
		SET b1=CONCAT(b1,if(b1<>'',', ',''),b2);
	END if;
	if b3<>'' then
		SET b1=CONCAT(b1,if(b1<>'',', ',''),b3);
	END if;
	SET b1=REGEXP_REPLACE(trim(b1),',$','');
	RETURN SUBSTRING(REPLACE(b1,'#',' '),1,255);
	#REPLACE(REPLACE(TRIM(CONCAT(if(b1!='',CONCAT(TRIM(b1),' '),''),if(b2!='',CONCAT(TRIM(b2),' '),''),if(b3!='',TRIM(b3),''))),' ',', '),'æ',' ');
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.binekMarkaGetir
DELIMITER //
CREATE FUNCTION `binekMarkaGetir`(
	`deger` TEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
	SET @a=LOCATE('>',deger,1);
	if @a=0 then
		RETURN NULL;
	ELSE
		if SUBSTRING_INDEX(deger,'>',1)='BİNEK' then 
			RETURN SUBSTRING_INDEX(SUBSTR(deger,@a+1),'>',1);
		ELSE 
			RETURN NULL;
		END if;
	END if;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.detailVer
DELIMITER //
CREATE FUNCTION `detailVer`(
	`firma` VARCHAR(50),
	`p1` TINYTEXT,
	`p2` TINYTEXT,
	`p3` TINYTEXT,
	`p4` TINYTEXT,
	`p5` TINYTEXT,
	`p6` TINYTEXT,
	`p7` TINYTEXT,
	`p8` TINYTEXT,
	`p9` TINYTEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
	DECLARE detay TEXT DEFAULT '';
	if firma = 'motorasin' then
		SET detay = CONCAT('<p>Stok Kodu: ',detayDegisim(p1),'</p><p>Stok Adı: ',detayDegisim(p2),'</p><p>Parça Markası: ',detayDegisim(p3),'</p><p>Önceki Kodu:</p><p>Araç: ',detayDegisim(p4),'&sol;',detayDegisim(p5),'</p><p>Ürün Grubu:</p><p>Ürün Tipi:</p><p>Oem Numarası: ',detayDegisim(p6),'</p><p>Eşdeğer Parça: </p>');
	ELSEIF firma = 'dinamik' then
		SET detay = CONCAT('<p>Stok Kodu: ',detayDegisim(p1),'</p><p>Stok Adı: ',detayDegisim(p2),'</p><p>Parça Markası: ',detayDegisim(p3),'</p><p>Önceki Kodu: ',detayDegisim(p4),'</p><p>Araç: ',detayDegisim(kull1A(p5)),'&sol;',detayDegisim(kull1B(p5)),'</p><p>Ürün Grubu: ',detayDegisim(p6),'</p><p>Ürün Tipi: ',detayDegisim(p7),'</p><p>Oem Numarası: ',detayDegisim(p8),'</p><p>Eşdeğer Parça: ',detayDegisim(p9),'</p>');
	END if;
	RETURN CONCAT('<span style=\'font-family: Tahoma, Geneva, sans-serif; font-size: 14pt\'>',detay,'</span>');
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.detayDegisim
DELIMITER //
CREATE FUNCTION `detayDegisim`(
	`yazi` TEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN

	RETURN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(yazi,'&','&amp;'),'"','&quot;'),'/','&sol;'),'<','&lt;'),'>','&gt;'),'\'','&apos;'),'\n',' ');
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.durumFilitre
DELIMITER //
CREATE FUNCTION `durumFilitre`(
	`durum` TINYTEXT
) RETURNS tinytext CHARSET utf8
    DETERMINISTIC
BEGIN
return concat(if(INSTR(durum,'S')>0,'S',''),if(INSTR(durum,'A')>0,'A',''),if(INSTR(durum,'M')>0,'M',''),if(INSTR(durum,'P')>0,'P',''));
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.durumNedir
DELIMITER //
CREATE FUNCTION `durumNedir`(
	`sku` VARCHAR(50),
	`usdKur` FLOAT,
	`eurKur` FLOAT,
	`fiyatDegisimAltLimitYuzdesi` FLOAT
) RETURNS varchar(8) CHARSET utf8
    DETERMINISTIC
BEGIN

#mulahazada kullanılıyordu. mulahaza kullanılmıyor.

DECLARE donecek VARCHAR(8) DEFAULT 'E';
DECLARE x1 TEXT DEFAULT NULL;
DECLARE x2 TEXT;
DECLARE m1 INT;
DECLARE m2 INT;
DECLARE b1 VARCHAR(50);
DECLARE b2 VARCHAR(50);
DECLARE cu1 VARCHAR(10);
DECLARE cu2 VARCHAR(10);
DECLARE ca1 VARCHAR(255);
DECLARE ca2 VARCHAR(255);
DECLARE sa1 INT;
DECLARE sa2 INT;
DECLARE pr1 FLOAT;
DECLARE pr2 FLOAT;
DECLARE firma VARCHAR(75);
DECLARE resimProductID INT DEFAULT NULL;
DECLARE dsOrder1 INT DEFAULT NULL;
DECLARE dsOrder2 INT DEFAULT NULL;
DECLARE dsc1 INT DEFAULT NULL;
DECLARE dsc2 INT DEFAULT NULL;

SELECT CONCAT(SUBSTRING(ortak.label,1,255),ortak.barcode,ortak.stock_type),
	CONCAT(ideasoft.label,ideasoft.barcode,ideasoft.stock_type),
    ortak.brand,
    ideasoft.brand,
    ortak.category_path,
    ideasoft.category_path,
    ortak.currency_abbr,
    ideasoft.currency_abbr,
    ortak.paket_miktari,
	 ideasoft.paket_miktari,
    ortak.stok_amount,
    ideasoft.stok_amount,
    ortak.price,
    ideasoft.price,
    ortak.nereden,
    ortak.discountedSortOrder,
    ideasoft.discountedSortOrder,
    ortak.discount,
    ideasoft.discount
    INTO x1,x2,b1,b2,ca1,ca2,cu1,cu2,m1,m2,sa1,sa2,pr1,pr2,firma,dsOrder1,dsOrder2,dsc1,dsc2
    FROM ortak LEFT JOIN ideasoft ON ortak.stok_kodu=ideasoft.stok_kodu WHERE ortak.stok_kodu=sku LIMIT 1;

if x2 IS NOT NULL then
   SET donecek='Y';
	if x1<>x2 then 
		SET donecek=CONCAT(donecek,'D'); # label+barkod+stokType
	END if;
	if sa1<>sa2 then 
		SET donecek=CONCAT(donecek,'S'); #stok miktarı
	END if;
	if b1<>b2 then 
		SET donecek=CONCAT(donecek,'B'); #marka
	END if;
	if cu1<>cu2 then 
		SET donecek=CONCAT(donecek,'A'); #para birimi
	END if;
	if ca1<>ca2 then 
		SET donecek=CONCAT(donecek,'C'); #katagori
	END if;
	if m1<>m2 then 
		SET donecek=CONCAT(donecek,'M'); #paket miktarı
	END if;
	if dsc1<>dsc2 then 
		SET donecek=CONCAT(donecek,'P'); #indirim yüzdesi değişince fiyat değişim gibi kabul et.
	END if;
	if fiyatDegisimYuzdesi(cu1,pr1,cu2,pr2,usdKur,eurKur)>=fiyatDegisimAltLimitYuzdesi then
		SET donecek=CONCAT(donecek,'P'); # fiyat
	END if;
	if dsOrder1 IS NULL then
		if dsOrder2 IS NOT NULL then 
			SET donecek=CONCAT(donecek,'O');
		END if;
	ELSE
	 	if dsOrder2 IS NULL then
			SET donecek=CONCAT(donecek,'O');
		ELSE 
			if dsOrder1<>dsOrder2 then
				SET donecek=CONCAT(donecek,'O');
			END if;
		END if;
	END if;
	
	/*
	SELECT 
		product_images.product_id into resimProductID FROM 
		ortak 
			INNER JOIN 
				(products inner join product_images ON products.id=product_images.product_id) 
			ON products.sku=ortak.stok_kodu 
	WHERE ortak.stok_kodu=sku; # and resimler.resim_base64!='';
	
	if resimProductID is NULL then	
	#if resimVarmiGelen(sku, firma) AND NOT resimVarmiGiden(sku) then
		SET donecek=CONCAT(donecek,'I');
	END if;
	*/	
	
	
	if donecek<>'Y' then
   	SET donecek=SUBSTR(donecek,2);
	END if;
	
END if;
RETURN donecek;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.fiyatDegisimYuzdesi
DELIMITER //
CREATE FUNCTION `fiyatDegisimYuzdesi`(
	`eskiBirim` VARCHAR(3),
	`eskiFiyat` FLOAT,
	`yeniBirim` VARCHAR(3),
	`yeniFiyat` FLOAT,
	`USD` FLOAT,
	`EUR` FLOAT
) RETURNS float
    DETERMINISTIC
BEGIN
	RETURN 100*ABS(TL_Fiyat_Ver(eskiBirim,eskiFiyat,USD,EUR)-TL_Fiyat_Ver(yeniBirim,yeniFiyat,USD,EUR))/least(TL_Fiyat_Ver(eskiBirim,eskiFiyat,USD,EUR),TL_Fiyat_Ver(yeniBirim,yeniFiyat,USD,EUR));
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.fiyatDinamik
DELIMITER //
CREATE FUNCTION `fiyatDinamik`(
	`fiyat` FLOAT
) RETURNS float
    DETERMINISTIC
BEGIN
	IF fiyat < 50 THEN 
   	RETURN fiyat;
	ELSEIF fiyat < 100 THEN
   	RETURN fiyat * 0.9;
	ELSEIF fiyat < 250 THEN
		RETURN fiyat * 0.85;
	ELSEIF fiyat < 500 THEN
		RETURN fiyat * 0.8;
	ELSEIF fiyat < 1000 THEN
		RETURN fiyat * 0.75;
	ELSEIF fiyat < 10000 THEN 
		RETURN fiyat * 0.7;
	ELSEIF fiyat < 100000 THEN
		RETURN fiyat * 0.65;
	ELSE 
		RETURN fiyat * 0.65;
	END IF;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.getBrandName
DELIMITER //
CREATE FUNCTION `getBrandName`(
	`brandId` INT(11)
) RETURNS tinytext CHARSET utf8
    DETERMINISTIC
BEGIN
	DECLARE t TINYTEXT DEFAULT NULL;
	SELECT brands.name INTO t FROM brands WHERE brands.id=brandId LIMIT 1;
	if t IS NULL then
 		RETURN '';
	ELSE
 		RETURN t;
	end if;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.getCurrencyAbbr
DELIMITER //
CREATE FUNCTION `getCurrencyAbbr`(
	`currencyId` INT(11)
) RETURNS tinytext CHARSET utf8
    DETERMINISTIC
BEGIN
	DECLARE t TINYTEXT DEFAULT NULL;
	SELECT currencies.abbr INTO t FROM currencies WHERE currencies.id=currencyId LIMIT 1;
	if t IS NULL then
		RETURN '';
	ELSE
		RETURN t;
	end if;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.getPaketMiktari
DELIMITER //
CREATE FUNCTION `getPaketMiktari`(
	`productId` INT
) RETURNS int(4)
    DETERMINISTIC
BEGIN
	DECLARE t INT(11) DEFAULT NULL;
	DECLARE tt INT(4) DEFAULT NULL;
	SELECT purchase_limitation_items.limitation_id INTO t FROM purchase_limitation_items WHERE purchase_limitation_items.product_id=productId LIMIT 1;
	if t IS NULL then
 		RETURN 1;
	ELSE
 		SELECT purchase_limitations.minimumLimit INTO tt FROM purchase_limitations WHERE purchase_limitations.id=t;
		if tt IS NULL then
  			RETURN 1;
		ELSE
  			RETURN tt;
		end if;
	end if;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.getPath
DELIMITER //
CREATE FUNCTION `getPath`(
	`productID` INT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
	#DECLARE t TEXT DEFAULT '';
	DECLARE kategoriId INT DEFAULT NULL;
	DECLARE c TINYTEXT DEFAULT NULL;
	DECLARE path TEXT DEFAULT '';
	DECLARE p_id INT DEFAULT NULL;
	
	SELECT product_to_categories.category_id INTO kategoriId FROM product_to_categories WHERE product_id=productID;
	
	if kategoriId IS NOT NULL then
		REPEAT
			SET p_id=NULL;
			SELECT categories.name,categories.parent_id INTO c,p_id FROM categories WHERE categories.id=kategoriId;
			if c IS NOT NULL then
				SET path=CONCAT(c,if(path='','','>'),path);
				SET kategoriId=p_id;
			END if;
		UNTIL p_id IS NULL END REPEAT;
		
		
		/*if c1 IS NULL then
			RETURN '';#CONCAT('#HATA 1# Sebebi: Product_to_categories tablosunda bu ürüne ait bir category_id (', CONVERT(kategoriId, CHAR),') var. Fakat categories tablosunda böyle bir kategori yok.');
		ELSE
			if p_id IS NULL then
				RETURN c1;
			ELSE
				SELECT categories.name,categories.parent_id INTO c2,p_id FROM categories WHERE categories.id=p_id;
				if c2 IS NULL then
					RETURN '';#CONCAT('#HATA 2# Sebebi: Bu ürünün alt kategorisi (', CONVERT(kategoriId, CHAR),') var. Fakat ana kategorisi(ID:', CONVERT(p_id, CHAR),') categories tablosunda böyle bir kategori yok.');
				ELSEIF p_id IS NULL then
					RETURN CONCAT(c2,">",c1);
				ELSE
					RETURN '';#CONCAT('#HATA 3# Sebebi: Ürünün kategori yapısı 2 kategoriden fazla. Ana kategorinin parent_id NULL olması gerekirken parent_id ',CONVERT(p_id, CHAR),' olarak dönüyor.');
				END if;
			END if;
		END if;*/
	END if;
	RETURN path;
	 
	#CALL getPath_(kategoriID, t);
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.kull1A
DELIMITER //
CREATE FUNCTION `kull1A`(
	`kull1` TEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
IF INSTR(kull1,'TRUCK')>0 THEN
	RETURN 'AĞIR VASITA';
ELSEIF INSTR(kull1,'TICARI')>0 THEN
   RETURN 'HAFİF TİCARİ';
ELSE 
   RETURN 'BİNEK';
END IF;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.kull1B
DELIMITER //
CREATE FUNCTION `kull1B`(
	`kull1` TEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
IF INSTR(kull1,'TRUCK')>0 THEN
	RETURN TRIM(SUBSTRING_INDEX(kull1,'TRUCK',1));
ELSEIF INSTR(kull1,'TICARI')>0 THEN
	RETURN TRIM(SUBSTRING_INDEX(kull1,'TICARI',1));
ELSE 
	RETURN TRIM(kull1);
END IF;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.motorasinHesaplananFiyat
DELIMITER //
CREATE FUNCTION `motorasinHesaplananFiyat`(
	`fiyat` FLOAT,
	`fiyatBirimi` VARCHAR(4),
	`donecekBirim` VARCHAR(4),
	`usdKur` FLOAT,
	`eurKur` FLOAT
) RETURNS float
    DETERMINISTIC
BEGIN
	DECLARE m_fiyat FLOAT DEFAULT 0;
	SET m_fiyat=TL_Fiyat_Ver(fiyatBirimi,fiyat, usdKur, eurKur);
	IF m_fiyat>100000 THEN
		SET m_fiyat=m_fiyat*1.1;
	ELSEIF m_fiyat>10000 THEN
		SET m_fiyat=m_fiyat*1.125;
	ELSEIF m_fiyat>1000 THEN
		SET m_fiyat=m_fiyat*1.175;
	ELSEIF m_fiyat>500 THEN
		SET m_fiyat=m_fiyat*1.225;
	ELSEIF m_fiyat>250 THEN
		SET m_fiyat=m_fiyat*1.25;
	ELSEIF m_fiyat>100 THEN
		SET m_fiyat=m_fiyat*1.3;
	ELSEIF m_fiyat>50 THEN
		SET m_fiyat=m_fiyat*1.35;
	ELSE 
		SET m_fiyat=m_fiyat*1.35;
	END IF;	
	
	IF donecekBirim='EUR' THEN 
		SET m_fiyat=m_fiyat/eurKur;
	ELSEIF donecekBirim='USD' THEN
		SET m_fiyat=m_fiyat/usdKur;
	END IF;
	
	RETURN m_fiyat;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.stockAmountDinamik
DELIMITER //
CREATE FUNCTION `stockAmountDinamik`(
	`varyok1` VARCHAR(5),
	`varyok2` VARCHAR(5),
	`varyok3` VARCHAR(5),
	`paketMiktari` INT(3)
) RETURNS int(4)
    DETERMINISTIC
BEGIN
   DECLARE adet INT DEFAULT 0;
   SET adet=if(varyok1='VAR',paketMiktari,0)+if(varyok2='VAR',paketMiktari,0)+if(varyok3='VAR',paketMiktari,0);
   RETURN adet;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.stockAmountMotorasin
DELIMITER //
CREATE FUNCTION `stockAmountMotorasin`(
	`quantity` VARCHAR(10),
	`minOrder` FLOAT
) RETURNS int(4)
    DETERMINISTIC
BEGIN
IF quantity='VAR' THEN
   IF minOrder<5 THEN
      RETURN 5;
	ELSE
      RETURN FLOOR(minOrder);
	END IF;
ELSE
  RETURN FLOOR(CONVERT(quantity,FLOAT));
END IF;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.stockTypeDinamik
DELIMITER //
CREATE FUNCTION `stockTypeDinamik`(
	`olcuBirimi` VARCHAR(10)
) RETURNS varchar(15) CHARSET utf8
    DETERMINISTIC
BEGIN
	IF olcuBirimi='AD' THEN
   	RETURN 'Piece';
	ELSEIF olcuBirimi='TK' THEN
   	RETURN 'Person';
	ELSEIF olcuBirimi='MT' THEN
   	RETURN 'metre';
	ELSEIF olcuBirimi='KG' THEN
   	RETURN 'kg';
	ELSEIF olcuBirimi='PK' THEN
   	RETURN 'Package';
	ELSE
   	RETURN 'Person';
	END IF;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.TL_Fiyat_Ver
DELIMITER //
CREATE FUNCTION `TL_Fiyat_Ver`(
	`paraBirimi` VARCHAR(3),
	`fiyat` FLOAT,
	`usdKur` FLOAT,
	`eurKur` FLOAT
) RETURNS float
    DETERMINISTIC
BEGIN
if paraBirimi='TL' then
	RETURN fiyat;
ELSEIF paraBirimi='EUR' then
	RETURN fiyat*eurKur;
ELSEIF paraBirimi='USD' then
	RETURN fiyat*usdKur;
ELSE
	RETURN fiyat;
END if;
END//
DELIMITER ;

-- fonksiyon yapısı dökülüyor nettengelsin.yeniBinekGetir
DELIMITER //
CREATE FUNCTION `yeniBinekGetir`(
	`deger` TEXT
) RETURNS text CHARSET utf8
    DETERMINISTIC
BEGIN
	DECLARE binekYeniDeger TEXT DEFAULT NULL;
	SET @a=binekMarkaGetir(deger);
	if @a IS NULL then
		RETURN deger;
	ELSE
		SELECT binekdegisim.binek INTO binekYeniDeger FROM binekdegisim WHERE binekdegisim.marka=@a LIMIT 1;
		if binekYeniDeger IS NULL then 
			SET binekYeniDeger='BİNEK 3.GRUP';
		END if;
		RETURN REPLACE(deger,'BİNEK',binekYeniDeger);
	END if;
END//
DELIMITER ;

-- tetikleyici yapısı dökülüyor nettengelsin.resimUzunluk
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='';
DELIMITER //
CREATE TRIGGER `resimUzunluk` BEFORE UPDATE ON `resimler` FOR EACH ROW BEGIN
if OLD.resim_base64<>NEW.resim_base64 then
	SET new.resim_uzunluk=FLOOR(LENGTH(new.resim_base64)*3/4);
END if;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
