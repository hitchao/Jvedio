SELECT LabelName,Count(LabelName) as Count  from metadata_to_label 
JOIN metadata on metadata.DataID=metadata_to_label.DataID
where metadata.DBId=1 and metadata.DataType=0
GROUP BY LabelName