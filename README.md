This is an example on how to call cloud custom action apis.

The action in cloud is available in the project and it's used to allow inserting data logger into freshtrack database. 

The custom action is the following:


In cloud, all custom actions have a JSONB paameter, called p_input.

Inside the action, p_input data can be used to perform various operations, suh as the following:

    INSERT INTO logger (ref_logger_id, ref_track_id, company, fresh_track_link, fresh_track_link_type)
    VALUES (p_input->>'ref_logger_id', p_input->>'ref_track_id', p_input->>'company',  p_input->>'freshtrack_link',  p_input->>'freshtrack_link_type')
    RETURNING to_jsonb(logger.*) INTO _record;
    RETURN fts_sys_build_result(fts_sys_result_data('record', _record));


In order to use the API, you will need to specify in the .env the following:

FTS_CLOUD_API_ENDPOINT_URL=https://demo.freshtrack.com
FTS_CLOUD_API_USER_EMAIL=your user email
FTS_CLOUD_API_USER_PASSWORD your user pass
