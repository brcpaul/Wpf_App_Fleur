DROP DATABASE IF EXISTS fleur; 
CREATE DATABASE IF NOT EXISTS fleur; 
USE fleur;

DROP TABLE IF EXISTS client;
CREATE  TABLE IF NOT EXISTS client (
	id_client VARCHAR(40),				#ID number of the customer
    nom VARCHAR(40),					#Name of the customer
    prenom VARCHAR(40),					#First name of the customer
    tel VARCHAR(40),					#Phone numer of the customer
    mail VARCHAR(40),					#Mail of the customer
    mdp VARCHAR(40),					#Password of the customer
    adresse_factu VARCHAR(100),			#Billing adress of the customer
    num_carte VARCHAR(40),				#Card number of the customer
    statut VARCHAR(40),					#Status for fidelity offer
    PRIMARY KEY (id_client));

DROP TABLE IF EXISTS produit;
CREATE  TABLE IF NOT EXISTS produit (
	id_produit VARCHAR(40),				#ID number of product
    nom VARCHAR(40), 					#Name of the product
    prix float, 						#Price of the product
    disponibilite VARCHAR(40), 			#Disponibility of the product
    PRIMARY KEY (id_produit));

DROP TABLE IF EXISTS bouquet_standard;
CREATE  TABLE IF NOT EXISTS bouquet_standard (
	id_bs VARCHAR(40),					#ID number of standard package 
    nom VARCHAR(40),					#Name of the bouquet
    composition VARCHAR(100),			#Composition of the bouquet
    #disponibilite VARCHAR(40), 			#Disponibility of the bouquet
    prix float,							#The price per unity of the bouquet
    categorie VARCHAR(40),				#The category of the bouquet
	PRIMARY KEY (id_bs));

DROP TABLE IF EXISTS bouquet_perso;
CREATE  TABLE IF NOT EXISTS bouquet_perso (
	id_bp VARCHAR(40),					#ID number of personalised package 
    description_bp VARCHAR(100),		#Composition of the bouquet
    prix_max float,						#The price per unity of the bouquet
	PRIMARY KEY (id_bp));

DROP TABLE IF EXISTS boutique;
CREATE  TABLE IF NOT EXISTS boutique (
	id_boutique VARCHAR(40),			#ID number of the shop
    adresse VARCHAR(100),					#Place of the shop
	PRIMARY KEY (id_boutique));
    
DROP TABLE IF EXISTS composition_bouquet;
CREATE  TABLE IF NOT EXISTS composition_bouquet (
	id_bp VARCHAR(40),					#ID number of personalised package 
    id_produit VARCHAR(40),				#ID number of porduct
    quantite int, 						#Quantity of composition
    CONSTRAINT id_cb_bp FOREIGN KEY (id_bp) REFERENCES bouquet_perso (id_bp),
	CONSTRAINT id_cb_produit FOREIGN KEY (id_produit) REFERENCES produit (id_produit),
    PRIMARY KEY(id_bp, id_produit));
    
DROP TABLE IF EXISTS stock;
CREATE  TABLE IF NOT EXISTS stock (
	id_boutique VARCHAR(40),			#ID number of personalised package 
    id_produit VARCHAR(40),				#ID number of porduct
    quantite int, 						#Quantity of composition
    CONSTRAINT id_st_bp FOREIGN KEY (id_boutique) REFERENCES boutique (id_boutique),
	CONSTRAINT id_st_produit FOREIGN KEY (id_produit) REFERENCES produit (id_produit),
    PRIMARY KEY(id_boutique, id_produit));
    
DROP TABLE IF EXISTS commande;
CREATE  TABLE IF NOT EXISTS commande (
	id_commande VARCHAR(40),				#ID number of the order
    id_client VARCHAR(40),					#ID number of the customer 
    id_boutique VARCHAR(40),
    est_standard BOOLEAN,
    id_bouquet VARCHAR(40), 				#ID number of the bouquet
    adresse_livraison VARCHAR(100),			#Delivery adress of the order
    date_livraison date,					#Date of delivery of the order
    date_commande date, 					#Date of the order
    prix_tot float,							#Final price of the command
    etat VARCHAR(40), 						#State of the order
    CONSTRAINT id_number_client FOREIGN KEY (id_client) REFERENCES client (id_client) ON DELETE CASCADE,
    CONSTRAINT id_number_boutique FOREIGN KEY (id_boutique) REFERENCES boutique (id_boutique) ON DELETE CASCADE, 
	PRIMARY KEY (id_commande));
/*
CREATE USER 'bozo'@'localhost' IDENTIFIED BY 'bozo';
GRANT SELECT ON * TO 'bozo'@'localhost';
FLUSH PRIVILEGES;
*/
DELIMITER //     #permet la création du déclencheur (ou trigger) avec plusieurs instructions
CREATE TRIGGER verifier_statut_fidelite_client AFTER INSERT ON commande
FOR EACH ROW
BEGIN
    #Calcul du nombre total de bouquets achetés par mois
    SET @total_bouquets_mois = (
        SELECT COUNT(*)
        FROM commande
        WHERE id_client = NEW.id_client
        AND date_commande >= DATEADD(day,31,GETDATE())
    );

    #Mise à jour du statut du client en fonction du nombre de bouquets achetés par mois
    #Fidélité OR si le client achète plus de 5 bouquets par mois, alors une réduction de 15% est offerte sur chaque bouquet 
    IF @total_bouquets_mois > 5 THEN
        UPDATE client
        SET statut = 'OR'
        WHERE id_client = NEW.id_client;
    #Fidélité Bronze si le client achète en moyenne un bouquet par mois alors une réduction de 5% est offerte 
    ELSEIF @total_bouquets_mois >= 1 THEN
        UPDATE client
        SET statut = 'BRONZE'
        WHERE id_client = NEW.id_client;
    END IF;
END //
DELIMITER ;     #on rétablit le délimiteur à sa valeur par défaut ';'
