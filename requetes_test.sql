use fleur;
SELECT id_commande, MIN(co.id_client) as "Id client", MIN(cl.prenom) as "Prénom client", MIN(cl.nom) as "Nom client",MIN(co.prix_tot) as "Prix total", MIN(bs.composition) as "Composition standard", GROUP_CONCAT(CONCAT(cb.quantite," ",pr.nom)) as "Comosition perso" FROM commande co
LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
JOIN client cl ON cl.id_client=co.id_client
LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
GROUP BY id_commande;

#SELECT bs.nom, COUNT(c.id_bs) as nombre_commandes FROM bouquet_standard bs JOIN commande c ON bs.id_bs = c.id_bs GROUP BY bs.nom ORDER BY nombre_commandes DESC LIMIT 1;

SELECT bs.nom, COUNT(*) AS total_commandes
FROM bouquet_standard bs
JOIN commande c ON bs.id_bs = c.id_bouquet
WHERE c.est_standard = true
GROUP BY bs.id_bs
ORDER BY total_commandes DESC
LIMIT 1;

SELECT c.id_boutique, SUM(c.prix_tot) AS chiffre_affaires
FROM commande c
GROUP BY c.id_boutique
ORDER BY chiffre_affaires DESC
LIMIT 10;

select avg(prix) from bouquet_standard;

SELECT c.id_client, c.nom, c.prenom, c.tel, c.mail, c.adresse_factu, c.num_carte, c.statut FROM client c
                        INNER JOIN commande cmd ON c.id_client = cmd.id_client 
                        WHERE cmd.date_commande >= DATE_SUB(NOW(), INTERVAL 1 MONTH)
                        GROUP BY c.id_client HAVING COUNT(*) > 1;