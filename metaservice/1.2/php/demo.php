<?php

    // This code depends on the PHP Soap and PHP OpenSSL modules being available
    //
    // There appears to be a bug in PHP Soap that
    // incorrectly wraps WS-I compliant Arrays in
    // an object that carries the name of the
    // object type of the elements in the array.
    // This seems to occur only when SOAP_SINGLE_ELEMENT_ARRAYS
    // is enabled, but disabling it is not really an option
    // because that deserializes single element arrays into non-array properties.

    $username = "testuser";
    $password = "0123456789";

    // Set up the client
    $client = new SoapClient('https://services.companywebcast.com/meta/1.2/MetaService.svc?wsdl',
                             array('features' => SOAP_SINGLE_ELEMENT_ARRAYS));

    $searchparameters = new stdClass();
    $searchparameters->Username = $username;
    $searchparameters->Password = $password;
    $searchparameters->CustomerCode = null;
    $searchparameters->CustomerName = null;
    $searchparameters->TopicTitle = null;
    $searchparameters->SpeakerLastName = null;
    $searchparameters->WebcastTitle = null;
    $searchparameters->Reference = null;
    $searchparameters->TagNames = null;
    $searchparameters->QueryText = null;
    $searchparameters->PeriodFrom = null;
    $searchparameters->PeriodTo = null;
    $searchparameters->Status = null;
    $searchparameters->PageNumber = 0;
    $searchparameters->PageSize = 100;
    $searchparameters->Order = "StartDesc";

    //   Call the WebcastSearch Method on the MetaService
    $SearchResponse = $client->WebcastSearch($searchparameters);
    //   WebcastSearchResult contains an integer
    //   that indicates the success or failure of your request
    $SearchResult = $SearchResponse->WebcastSearchResult;
    //   WebcastSummaries contains a collection of WebcastSummary,
    //   which contains data that can be used for WebcastGet
    $Summaries = $SearchResponse->WebcastSummaries->WebcastSummary; //Php Soap bug

    //   Create an object that contains the parameters needed for calling WebcastGet
    $getparameters = new stdClass();
    $getparameters->Username = $username;
    $getparameters->Password = $password;
    $getparameters->Code = $Summaries[0]->Code;
    $getparameters->Language = $Summaries[0]->Languages->string[0]; //Php Soap bug

    //   Call the WebcastGet Method on the MetaService
    $GetResponse =  $client->WebcastGet($getparameters);
    //   WebcastGetResult contains an integer
    //   that indicates the success or failure of your request
    $GetResult = $GetResponse->WebcastGetResult;
    //   Webcast is an object that contains detailed information
    //   about a single webcast, in a single language
    $Webcast = $GetResponse->Webcast;
?>