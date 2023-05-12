use fleur;
SELECT id_commande, MIN(co.id_client) as "Id client", MIN(cl.prenom) as "Prénom client", MIN(cl.nom) as "Nom client",MIN(co.prix_tot) as "Prix total", MIN(bs.composition) as "Composition standard", GROUP_CONCAT(CONCAT(cb.quantite," ",pr.nom)) as "Comosition perso" FROM commande co
LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
JOIN client cl ON cl.id_client=co.id_client
LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
GROUP BY id_commande;

SELECT bs.nom, COUNT(c.id_bs) as nombre_commandes FROM bouquet_standard bs JOIN commande c ON bs.id_bs = c.id_bs GROUP BY bs.nom ORDER BY nombre_commandes DESC LIMIT 1;

SELECT bs.nom, COUNT(*) AS total_commandes
FROM bouquet_standard bs
JOIN commande c ON bs.id_bs = c.id_bouquet
WHERE c.est_standard = true
GROUP BY bs.id_bs
ORDER BY total_commandes DESC
LIMIT 1;