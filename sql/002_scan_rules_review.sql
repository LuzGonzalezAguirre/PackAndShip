-- Consulta de solo lectura: valores utilizados actualmente por Q-Wall.
-- Compartir resultados antes de habilitar el CRUD de las reglas.
SELECT extraction_mode, field_target, separator, separator_custom,
       value_position, COUNT(*) AS usage_count
FROM dbo.quality_scan_fields
GROUP BY extraction_mode, field_target, separator, separator_custom, value_position
ORDER BY extraction_mode, field_target, separator, value_position;

SELECT r.id AS rule_id, r.ssi_pn, r.bu_name, r.scan_count, r.requires_match,
       r.is_active, f.scan_index, f.sequence_order, f.extraction_mode,
       f.field_target, f.separator, f.separator_custom, f.value_position,
       f.segment_index, f.fixed_length, f.prefix_value, f.display_label
FROM dbo.quality_pn_scan_rules r
INNER JOIN dbo.quality_scan_fields f ON f.rule_id = r.id
ORDER BY r.bu_name, r.ssi_pn, f.scan_index, f.sequence_order;
