SELECT ActorID from actor_info
JOIN actor_name_to_metadatas 
on actor_info.ActorName=actor_name_to_metadatas.ActorName and
actor_info.NameFlag=actor_name_to_metadatas.NameFlag
where DataID in ( SELECT DataID FROM "metadata" where DBId ='4' and DataType='0') 
GROUP BY actor_name_to_metadatas.ActorName,actor_name_to_metadatas.NameFlag ;