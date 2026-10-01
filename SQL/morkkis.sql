DROP DATABASE IF EXISTS MorkkisDB;
CREATE DATABASE IF NOT EXISTS MorkkisDB;

USE MorkkisDB;

CREATE TABLE Asiakas
(
  AsiakasID INT NOT NULL AUTO_INCREMENT,
  Etunimi VARCHAR(30) NOT NULL,
  Sukunimi VARCHAR(30) NOT NULL,
  Sahkoposti VARCHAR(50) NOT NULL,
  Puhelin VARCHAR(15) NOT NULL,
  Osoite VARCHAR(50) NOT NULL,
  PRIMARY KEY (AsiakasID)
);

CREATE TABLE Toimipiste
(
  ToimipisteID INT NOT NULL AUTO_INCREMENT,
  Nimi VARCHAR(30) NOT NULL,
  Osoite VARCHAR(50) NOT NULL,
  Puhelin VARCHAR(15) NOT NULL,
  Sahkoposti VARCHAR(50) NOT NULL,
  PRIMARY KEY (ToimipisteID)
);

CREATE TABLE Palvelut
(
  PalveluID INT NOT NULL AUTO_INCREMENT,
  Nimi VARCHAR(30) NOT NULL,
  Hinta DECIMAL(10,2) NOT NULL,
  ToimipisteID INT NOT NULL,
  PRIMARY KEY (PalveluID),
  FOREIGN KEY (ToimipisteID) REFERENCES Toimipiste(ToimipisteID)
);

CREATE TABLE Mokki
(
  MokkiID INT NOT NULL AUTO_INCREMENT,
  Nimi VARCHAR(25) NOT NULL,
  Vuorokausihinta DECIMAL(10,2) NOT NULL,
  ToimipisteID INT NOT NULL,
  PRIMARY KEY (MokkiID),
  FOREIGN KEY (ToimipisteID) REFERENCES Toimipiste(ToimipisteID)
);

CREATE TABLE Varaus
(
  VarausID INT NOT NULL AUTO_INCREMENT,
  Alkupaivamaara DATE NOT NULL,
  Loppupaivamaara DATE NOT NULL,
  AsiakasID INT NOT NULL,
  MokkiID INT NOT NULL,
  ToimipisteID INT NOT NULL,
  PRIMARY KEY (VarausID, AsiakasID, MokkiID, ToimipisteID),
  FOREIGN KEY (AsiakasID) REFERENCES Asiakas(AsiakasID),
  FOREIGN KEY (MokkiID) REFERENCES Mokki(MokkiID),
  FOREIGN KEY (ToimipisteID) REFERENCES Toimipiste(ToimipisteID)
);

CREATE TABLE Varauksen_palvelut
(
  PalveluID INT NOT NULL,
  VarausID INT NOT NULL,
  PRIMARY KEY (PalveluID, VarausID),
  FOREIGN KEY (PalveluID) REFERENCES Palvelut(PalveluID),
  FOREIGN KEY (VarausID) REFERENCES Varaus(VarausID)
);

CREATE TABLE Lasku
(
  LaskuID INT NOT NULL AUTO_INCREMENT,
  Paivamaara DATE NOT NULL,
  Erapaiva DATE NOT NULL,
  Osoite VARCHAR(50) NOT NULL,
  Puhelin VARCHAR(15) NOT NULL,
  Sahkoposti VARCHAR(50) NOT NULL,
  Etunimi VARCHAR(30) NOT NULL,
  Sukunimi VARCHAR(30) NOT NULL,
  Toimipiste VARCHAR(50) NOT NULL,
  Mokki VARCHAR(30) NOT NULL,
  Mitatoitu BOOL NOT NULL,
  AsiakasID INT,
  VarausID INT,
  PRIMARY KEY (LaskuID),
  FOREIGN KEY (AsiakasID) REFERENCES Asiakas(AsiakasID) ON DELETE SET NULL,
  FOREIGN KEY (VarausID) REFERENCES Varaus(VarausID) ON DELETE SET NULL
);

CREATE TABLE Laskurivi
(
  Tuotenimi VARCHAR(50) NOT NULL,
  Hinta FLOAT NOT NULL,
  Maara INT NOT NULL,
  LaskuriviID INT NOT NULL AUTO_INCREMENT,
  LaskuID INT NOT NULL,
  PRIMARY KEY (LaskuriviID),
  FOREIGN KEY (LaskuID) REFERENCES Lasku(LaskuID)
);

INSERT INTO asiakas (etunimi, sukunimi, sahkoposti, puhelin, osoite) VALUES
	('Mikko', 'Virtanen', 'mikko.virtanen@gmail.com', '0401234567', 'Keskuskatu 10, Helsinki'),
  	('Laura', 'Korhonen', 'laura.korhonen@gmail.com', '0457654321', 'Puistotie 4, Espoo'),
  	('Antti', 'Niemi', 'antti.niemi@gmail.com', '0501112233', 'Koulutie 7, Tampere'),
  	('Emilia', 'Mäkinen', 'emilia.makinen@gmail.com', '0419876543', 'Rantakatu 12, Turku'),
  	('Juho', 'Lehtonen', 'juho.lehtonen@gmail.com', '0445566778', 'Asemakatu 2, Oulu'),
  	('Sanna', 'Heikkilä', 'sanna.heikkila@gmail.com', '0469988776', 'Torikatu 5, Jyväskylä');
  
INSERT INTO toimipiste (nimi, osoite, puhelin, sahkoposti) VALUES
	('Perälahti', 'Peränsuuntie 24, Perä', '0402438594', 'peranlahti@gmail.com'),
	('Kummakumpu', 'Kumpulantie 45, Kumpula', '0402438892', 'kummakumpu@gmail.com'),
	('Mutapuro', 'Purontie 8, Mutala', '0401001001', 'mutapuro@gmail.com'),
   ('Mölymäki', 'Mölynkatu 12, Ääne', '0412233445', 'molymaki@gmail.com'),
   ('Tylsälä', 'Hiljaisentie 3, Tylsälä', '0455566778', 'tylsala@gmail.com');

  
INSERT INTO palvelut (nimi, hinta, toimipisteID) VALUES
	('Porosafari', 100, 1),
	('Laskettelu', 65, 1),
	('Koiravaljakkoajelu', 150, 2),
	('Lumikenkäily', 25, 3),
	('Saunajooga', 45, 4),
	('Turvekylpy', 100, 5);
	
INSERT INTO mokki (nimi, vuorokausihinta, toimipisteID) VALUES
	('Pöpelikkö', 45, 1),
	('Möykky', 50, 1),
	('Karhunpesä', 100, 2),
	('Möksy', 35, 2),
	('Tupajumi', 70, 3),
	('Torppa', 30, 4),
	('Kalliopirtti', 55, 5),
	('Pahkatalo', 56, 5),
	('Pukama', 75, 1),
	('Nyppylä', 55, 2),
	('Sammalkota', 50, 3),
	('Syrhämä', 45, 3),
	('Ketunkolo', 55, 4),
	('Risukasa', 100, 4),
	('Eräjorma', 120, 5);
	
INSERT INTO varaus (alkupaivamaara, loppupaivamaara, asiakasID, mokkiID, toimipisteID) VALUES
	('2025-05-01', '2025-05-05', 1, 1, 1),
	('2025-05-21', '2025-05-25', 1, 1, 1),
	('2025-06-10', '2025-06-12', 2, 3, 2),
	('2025-07-20', '2025-07-22', 3, 5, 3),
	('2025-08-15', '2025-08-20', 4, 6, 4),
	('2025-09-01', '2025-09-04', 5, 7, 5),
	('2025-06-21', '2025-06-25', 1, 2, 1),
	('2025-07-05', '2025-07-10', 2, 9, 1);

INSERT INTO lasku (paivamaara, erapaiva, osoite, puhelin, sahkoposti, etunimi, sukunimi,  toimipiste, mokki, mitatoitu, asiakasID, varausID) VALUES
	('2025-05-01', '2025-05-15', 'Keskuskatu 10, Helsinki', '0401234567', 'mikko.virtanen@gmail.com', 'Mikko', 'Virtanen', 'Perälahti', 'Pöpelikkö', 0, 1, 1),
	('2025-05-21', '2025-06-04', 'Keskuskatu 10, Helsinki', '0401234567', 'mikko.virtanen@gmail.com', 'Mikko', 'Virtanen', 'Perälahti', 'Pöpelikkö', 0, 1, 2),
	('2025-06-10', '2025-06-24', 'Puistotie 4, Espoo', '0457654321', 'laura.korhonen@gmail.com', 'Laura', 'Korhonen', 'Kummakumpu', 'Karhunpesä', 0, 2, 3),
	('2025-07-20', '2025-08-03', 'Koulutie 7, Tampere', '0501112233', 'antti.niemi@gmail.com', 'Antti', 'Niemi', 'Mutapuro', 'Tupajumi', 0, 3, 4),
	('2025-08-15', '2025-08-29', 'Rantakatu 12, Turku', '0419876543', 'emilia.makinen@gmail.com', 'Emilia', 'Mäkinen', 'Mölymäki', 'Torppa', 0, 4, 5),
	('2025-09-01', '2025-09-15', 'Asemakatu 2, Oulu', '0445566778', 'juho.lehtonen@gmail.com', 'Juho', 'Lehtonen', 'Tylsälä', 'Kalliopirtti', 0, 5, 6),
	('2025-06-21', '2025-07-05', 'Keskuskatu 10, Helsinki', '0401234567', 'mikko.virtanen@gmail.com', 'Mikko', 'Virtanen', 'Perälahti', 'Möykky', 0, 1, 7),
	('2025-07-05', '2025-07-19', 'Puistotie 4, Espoo', '0457654321', 'laura.korhonen@gmail.com', 'Laura', 'Korhonen', 'Perälahti', 'Pukama', 0, 2, 8);

INSERT INTO varauksen_palvelut (palveluID, varausID) VALUES
	(2, 1), 
	(1, 2),  
	(3, 2), 
	(4, 3), 
	(5, 4),  
	(6, 5),
	(1, 6),
	(2, 6),
	(2, 7);  
	
INSERT INTO laskurivi (tuotenimi, hinta, maara, laskuID) VALUES
	('Vuokra', 225, 5, 1),
	('Vuokra', 225, 5, 2),
	('Vuokra', 300, 3, 3),
	('Vuokra', 210, 3, 4),
	('Vuokra', 180, 6, 5),
	('Vuokra', 165, 3, 6),
	('Vuokra', 250, 5, 7),
	('Vuokra', 450, 6, 8),
	('Laskettelu', 65, 1, 1),
	('Porosafari', 100, 1, 2),
	('Koiravaljakkoajelu', 25, 1, 2),
	('Lumikenkäily', 25, 1, 3),
	('Saunajooga', 45, 1, 4),
	('Turvekylpy', 100, 1, 5),
	('Porosafari', 100, 1, 6),
	('Laskettelu', 65, 1, 6),
	('Laskettelu', 65, 1, 7);