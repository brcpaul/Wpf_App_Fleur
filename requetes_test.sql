use fleur;
SELECT id_commande, MIN(co.id_client) as "Id client", MIN(cl.prenom) as "Prénom client", MIN(cl.nom) as "Nom client",MIN(co.prix_tot) as "Prix total", MIN(bs.composition) as "Composition standard", GROUP_CONCAT(CONCAT(cb.quantite," ",pr.nom)) as "Comosition perso" FROM commande co
LEFT JOIN bouquet_perso bp ON co.id_bouquet=bp.id_bp
LEFT JOIN bouquet_standard bs ON co.id_bouquet=bs.id_bs
JOIN client cl ON cl.id_client=co.id_client
LEFT JOIN composition_bouquet cb ON co.est_standard=false and cb.id_bp=co.id_bouquet
LEFT JOIN produit pr ON co.est_standard=false and cb.id_produit=pr.id_produit
GROUP BY id_commande