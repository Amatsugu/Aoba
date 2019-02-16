\c postgres postgres
DROP DATABASE IF EXISTS AobaDB;

CREATE DATABASE AobaBD WITH OWNER Aoba;
\c AobaDB postgres

CREATE TYPE MediaType AS ENUM ('Image', 'Audio', 'Code', 'Text', 'Video');

CREATE TABLE users
(
	id VARCHAR(22) PRIMARY KEY,
	username VARCHAR(100) NOT NULL,
	password VARCHAR(100) NOT NULL,
	claims VARCHAR(22)[],
	apiKeys VARCHAR(22)[],
	regTokens VARCHAR(22)[]
);

CREATE TABLE media
(
	id varchar(22) PRIMARY KEY,
	type MediaType NOT NULL,
	fileName varchar(100),
	mediaId OID NOT NULL,
	owner VARCHAR(22) REFERENCES users(id),
	views INTEGER,
);