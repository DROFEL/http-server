http rfc notes

http 1.0 spec - https://datatracker.ietf.org/doc/html/rfc1945#section-7.1
http 1.1 spec - https://datatracker.ietf.org/doc/html/rfc9112
http 2.0 spec - https://datatracker.ietf.org/doc/html/rfc9113
http 3.0 spec - https://datatracker.ietf.org/doc/html/rfc9114

http 1.0 

no http version in the header => use HTTP/0.9
version out is the same as version in

3.2.1 General Syntax

   URIs in HTTP can be represented in absolute form or relative to some
   known base URI [9], depending upon the context of their use. The two
   forms are differentiated by the fact that absolute URIs always begin
   with a scheme name followed by a colon.

       URI            = ( absoluteURI | relativeURI ) [ "#" fragment ]

       absoluteURI    = scheme ":" *( uchar | reserved )

       relativeURI    = net_path | abs_path | rel_path

       net_path       = "//" net_loc [ abs_path ]
       abs_path       = "/" rel_path
       rel_path       = [ path ] [ ";" params ] [ "?" query ]

       path           = fsegment *( "/" segment )
       fsegment       = 1*pchar
       segment        = *pchar

       params         = param *( ";" param )
       param          = *( pchar | "/" )

       scheme         = 1*( ALPHA | DIGIT | "+" | "-" | "." )
       net_loc        = *( pchar | ";" | "?" )
       query          = *( uchar | reserved )
       fragment       = *( uchar | reserved )

       pchar          = uchar | ":" | "@" | "&" | "=" | "+"
       uchar          = unreserved | escape
       unreserved     = ALPHA | DIGIT | safe | extra | national

       escape         = "%" HEX HEX
       reserved       = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+"
       extra          = "!" | "*" | "'" | "(" | ")" | ","
       safe           = "$" | "-" | "_" | "."
       unsafe         = CTL | SP | <"> | "#" | "%" | "<" | ">"
       national       = <any OCTET excluding ALPHA, DIGIT,
3.2.2 http URL

   The "http" scheme is used to locate network resources via the HTTP
   protocol. This section defines the scheme-specific syntax and
   semantics for http URLs.

       http_URL       = "http:" "//" host [ ":" port ] [ abs_path ]

       host           = <A legal Internet host domain name
                         or IP address (in dotted-decimal form),
                         as defined by Section 2.1 of RFC 1123>

       port           = *DIGIT

3.3  Date/Time Formats

   HTTP/1.0 applications have historically allowed three different
   formats for the representation of date/time stamps:

       Sun, 06 Nov 1994 08:49:37 GMT    ; RFC 822, updated by RFC 1123
       Sunday, 06-Nov-94 08:49:37 GMT   ; RFC 850, obsoleted by RFC 1036
       Sun Nov  6 08:49:37 1994         ; ANSI C's asctime() format

   The first format is preferred.
   HTTP/1.0 clients and servers that parse the date value should accept
   all three formats, though they must never generate the third
   (asctime) format.

   All HTTP/1.0 date/time stamps must be represented in Universal Time
   (UT), also known as Greenwich Mean Time (GMT), without exception.
   This is indicated in the first two formats by the inclusion of "GMT"
   as the three-letter abbreviation for time zone, and should be assumed
   when reading the asctime format.

       HTTP-date      = rfc1123-date | rfc850-date | asctime-date

       rfc1123-date   = wkday "," SP date1 SP time SP "GMT"
       rfc850-date    = weekday "," SP date2 SP time SP "GMT"
       asctime-date   = wkday SP date3 SP time SP 4DIGIT

       date1          = 2DIGIT SP month SP 4DIGIT
                        ; day month year (e.g., 02 Jun 1982)
       date2          = 2DIGIT "-" month "-" 2DIGIT
                        ; day-month-year (e.g., 02-Jun-82)
       date3          = month SP ( 2DIGIT | ( SP 1DIGIT ))
                        ; month day (e.g., Jun  2)

       time           = 2DIGIT ":" 2DIGIT ":" 2DIGIT
                        ; 00:00:00 - 23:59:59

       wkday          = "Mon" | "Tue" | "Wed"
                      | "Thu" | "Fri" | "Sat" | "Sun"

       weekday        = "Monday" | "Tuesday" | "Wednesday"
                      | "Thursday" | "Friday" | "Saturday" | "Sunday"

       month          = "Jan" | "Feb" | "Mar" | "Apr"
                      | "May" | "Jun" | "Jul" | "Aug"
                      | "Sep" | "Oct" | "Nov" | "Dec"

3.4  Character Sets
     charset = "US-ASCII"
             | "ISO-8859-1" | "ISO-8859-2" | "ISO-8859-3"
             | "ISO-8859-4" | "ISO-8859-5" | "ISO-8859-6"
             | "ISO-8859-7" | "ISO-8859-8" | "ISO-8859-9"
             | "ISO-2022-JP" | "ISO-2022-JP-2" | "ISO-2022-KR"
             | "UNICODE-1-1" | "UNICODE-1-1-UTF-7" | "UNICODE-1-1-UTF-8"
             | token
    
    Although HTTP allows an arbitrary token to be used as a charset
   value, any token that has a predefined value within the IANA
   Character Set registry [15] must represent the character set defined

   by that registry. Applications should limit their use of character
   sets to those defined by the IANA registry.

   The character set of an entity body should be labelled as the lowest
   common denominator of the character codes used within that body, with
   the exception that no label is preferred over the labels US-ASCII or
   ISO-8859-1.

3.5  Content Codings
       content-coding = "x-gzip" | "x-compress" | token

       Note: For future compatibility, HTTP/1.0 applications should
       consider "gzip" and "compress" to be equivalent to "x-gzip"
       and "x-compress", respectively.

   All content-coding values are case-insensitive. HTTP/1.0 uses
   content-coding values in the Content-Encoding (Section 10.3) header
   field. 
    Note that a single program may be
   capable of decoding multiple content-coding formats. Two values are
   defined by this specification:

   x-gzip
       An encoding format produced by the file compression program
       "gzip" (GNU zip) developed by Jean-loup Gailly. This format is
       typically a Lempel-Ziv coding (LZ77) with a 32 bit CRC.

   x-compress
       The encoding format produced by the file compression program
       "compress". This format is an adaptive Lempel-Ziv-Welch coding
       (LZW).

3.6  Media Types

   HTTP uses Internet Media Types [13] in the Content-Type header field
   (Section 10.5) in order to provide open and extensible data typing.

       media-type     = type "/" subtype *( ";" parameter )
       type           = token
       subtype        = token

   Parameters may follow the type/subtype in the form of attribute/value
   pairs.

       parameter      = attribute "=" value
       attribute      = token
       value          = token | quoted-string
    
    The type, subtype, and parameter attribute names are case-
   insensitive. Parameter values may or may not be case-sensitive,
   depending on the semantics of the parameter name. LWS must not be
   generated between the type and subtype, nor between an attribute and
   its value. Upon receipt of a media type with an unrecognized
   parameter, a user agent should treat the media type as if the
   unrecognized parameter and its value were not present.
    Media-type values are registered with the Internet Assigned Number
   Authority (IANA [15]). The media type registration process is
   outlined in RFC 1590 [13]. Use of non-registered media types is
   discouraged.
3.6.1 Canonicalization and Text Defaults
   Media subtypes of the "text" type use CRLF as the text line break
   when in canonical form. However, HTTP allows the transport of text
   media with plain CR or LF alone representing a line break when used
   consistently within the Entity-Body. HTTP applications must accept
   CRLF, bare CR, and bare LF as being representative of a line break in
   text media received via HTTP.

3.6.2 Multipart Types

   MIME provides for a number of "multipart" types -- encapsulations of
   several entities within a single message's Entity-Body. The multipart
   types registered by IANA [15] do not have any special meaning for
   HTTP/1.0, though user agents may need to understand each type in
   order to correctly interpret the purpose of each body-part. An HTTP
   user agent should follow the same or similar behavior as a MIME user
   agent does upon receipt of a multipart type. HTTP servers should not
   assume that all HTTP clients are prepared to handle multipart types.

   All multipart types share a common syntax and must include a boundary
   parameter as part of the media type value. The message body is itself
   a protocol element and must therefore use only CRLF to represent line
   breaks between body-parts. Multipart body-parts may contain HTTP
   header fields which are significant to the meaning of that part.

3.7  Product Tokens


   Product tokens are used to allow communicating applications to
   identify themselves via a simple product token, with an optional
   slash and version designator. Most fields using product tokens also
   allow subproducts which form a significant part of the application to
   be listed, separated by whitespace. By convention, the products are
   listed in order of their significance for identifying the
   application.

       product         = token ["/" product-version]
       product-version = token

   Examples:

       User-Agent: CERN-LineMode/2.15 libwww/2.17b3

       Server: Apache/0.8.4

    Product tokens should be short and to the point -- use of them for
    advertizing or other non-essential information is explicitly
    forbidden. Although any token character may appear in a product-
    version, this token should only be used for a version identifier
    (i.e., successive versions of the same product should only differ in
    the product-version portion of the product value).

4.1  Message Types

   HTTP messages consist of requests from client to server and responses
   from server to client.

       HTTP-message   = Simple-Request           ; HTTP/0.9 messages
                      | Simple-Response
                      | Full-Request             ; HTTP/1.0 messages
                      | Full-Response

   Full-Request and Full-Response use the generic message format of RFC
   822 [7] for transferring entities. Both messages may include optional
   header fields (also known as "headers") and an entity body. The
   entity body is separated from the headers by a null line (i.e., a
   line with nothing preceding the CRLF).

       Full-Request   = Request-Line             ; Section 5.1
                        *( General-Header        ; Section 4.3
                         | Request-Header        ; Section 5.2
                         | Entity-Header )       ; Section 7.1
                        CRLF
                        [ Entity-Body ]          ; Section 7.2

       Full-Response  = Status-Line              ; Section 6.1
                        *( General-Header        ; Section 4.3
                         | Response-Header       ; Section 6.2
                         | Entity-Header )       ; Section 7.1
                        CRLF
                        [ Entity-Body ]          ; Section 7.2

   Simple-Request and Simple-Response do not allow the use of any header
   information and are limited to a single request method (GET).

       Simple-Request  = "GET" SP Request-URI CRLF

       Simple-Response = [ Entity-Body ]

   Use of the Simple-Request format is discouraged because it prevents
   the server from identifying the media type of the returned entity.

4.2  Message Headers

   HTTP header fields, which include General-Header (Section 4.3),
   Request-Header (Section 5.2), Response-Header (Section 6.2), and
   Entity-Header (Section 7.1) fields, follow the same generic format as
   that given in Section 3.1 of RFC 822 [7]. Each header field consists
   of a name followed immediately by a colon (":"), a single space (SP)
   character, and the field value. Field names are case-insensitive.
   Header fields can be extended over multiple lines by preceding each
   extra line with at least one SP or HT, though this is not
   recommended.

       HTTP-header    = field-name ":" [ field-value ] CRLF

       field-name     = token
       field-value    = *( field-content | LWS )

       field-content  = <the OCTETs making up the field-value
                        and consisting of either *TEXT or combinations
                        of token, tspecials, and quoted-string>

   The order in which header fields are received is not significant.
   However, it is "good practice" to send General-Header fields first,
   followed by Request-Header or Response-Header fields prior to the
   Entity-Header fields.

   Multiple HTTP-header fields with the same field-name may be present
   in a message if and only if the entire field-value for that header
   field is defined as a comma-separated list [i.e., #(values)]. It must
   be possible to combine the multiple header fields into one "field-
   name: field-value" pair, without changing the semantics of the
   message, by appending each subsequent field-value to the first, each
   separated by a comma.

5. Request

   A request message from a client to a server includes, within the
   first line of that message, the method to be applied to the resource,
   the identifier of the resource, and the protocol version in use. For
   backwards compatibility with the more limited HTTP/0.9 protocol,
   there are two valid formats for an HTTP request:

       Request        = Simple-Request | Full-Request

       Simple-Request = "GET" SP Request-URI CRLF

       Full-Request   = Request-Line             ; Section 5.1
                        *( General-Header        ; Section 4.3
                         | Request-Header        ; Section 5.2
                         | Entity-Header )       ; Section 7.1
                        CRLF
                        [ Entity-Body ]          ; Section 7.2

   If an HTTP/1.0 server receives a Simple-Request, it must respond with
   an HTTP/0.9 Simple-Response. An HTTP/1.0 client capable of receiving
   a Full-Response should never generate a Simple-Request.

5.1  Request-Line

   The Request-Line begins with a method token, followed by the
   Request-URI and the protocol version, and ending with CRLF. The
   elements are separated by SP characters. No CR or LF are allowed
   except in the final CRLF sequence.

    Request-Line = Method SP Request-URI SP HTTP-Version CRLF
   Note that the difference between a Simple-Request and the Request-
   Line of a Full-Request is the presence of the HTTP-Version field and
   the availability of methods other than GET.

5.1.1 Method

   The Method token indicates the method to be performed on the resource
   identified by the Request-URI. The method is case-sensitive.

       Method         = "GET"                    ; Section 8.1
                      | "HEAD"                   ; Section 8.2
                      | "POST"                   ; Section 8.3
                      | extension-method

       extension-method = token

   The list of methods acceptable by a specific resource can change
   dynamically; the client is notified through the return code of the
   response if a method is not allowed on a resource. Servers should
   return the status code 501 (not implemented) if the method is
   unrecognized or not implemented.

   The methods commonly used by HTTP/1.0 applications are fully defined
   in Section 8.

5.1.2 Request-URI

   The Request-URI is a Uniform Resource Identifier (Section 3.2) and
   identifies the resource upon which to apply the request.

       Request-URI    = absoluteURI | abs_path

   The two options for Request-URI are dependent on the nature of the
   request.

   The absoluteURI form is only allowed when the request is being made
   to a proxy. The proxy is requested to forward the request and return
   the response. An example
   Request-Line would be:

       GET http://www.w3.org/pub/WWW/TheProject.html HTTP/1.0

   The most common form of Request-URI is that used to identify a
   resource on an origin server or gateway. In this case, only the
   absolute path of the URI is transmitted (see Section 3.2.1,
   abs_path). 

       GET /pub/WWW/TheProject.html HTTP/1.0

   Note that the absolute
   path cannot be empty; if none is present in the original URI, it must
   be given as "/" (the server root).

   The Request-URI is transmitted as an encoded string, where some
   characters may be escaped using the "% HEX HEX" encoding defined by
   RFC 1738 [4]. The origin server must decode the Request-URI in order
   to properly interpret the request.

5.2  Request Header Fields

   The request header fields allow the client to pass additional
   information about the request, and about the client itself, to the
   server. These fields act as request modifiers, with semantics
   equivalent to the parameters on a programming language method
   (procedure) invocation.

       Request-Header = Authorization            ; Section 10.2
                      | From                     ; Section 10.8
                      | If-Modified-Since        ; Section 10.9
                      | Referer                  ; Section 10.13
                      | User-Agent               ; Section 10.15

    Unrecognized header fields are treated as
   Entity-Header fields.

6.  Response

   After receiving and interpreting a request message, a server responds
   in the form of an HTTP response message.

       Response        = Simple-Response | Full-Response

       Simple-Response = [ Entity-Body ]

       Full-Response   = Status-Line             ; Section 6.1
                         *( General-Header       ; Section 4.3
                          | Response-Header      ; Section 6.2
                          | Entity-Header )      ; Section 7.1
                         CRLF
                         [ Entity-Body ]         ; Section 7.2

   A Simple-Response should only be sent in response to an HTTP/0.9
   Simple-Request or if the server only supports the more limited
   HTTP/0.9 protocol. If a client sends an HTTP/1.0 Full-Request and
   receives a response that does not begin with a Status-Line, it should
   assume that the response is a Simple-Response and parse it
   accordingly. Note that the Simple-Response consists only of the
   entity body and is terminated by the server closing the connection.

6.1  Status-Line

   The first line of a Full-Response message is the Status-Line,
   consisting of the protocol version followed by a numeric status code
   and its associated textual phrase, with each element separated by SP
   characters. No CR or LF is allowed except in the final CRLF sequence.

       Status-Line = HTTP-Version SP Status-Code SP Reason-Phrase CRLF

   Since a status line always begins with the protocol version and
   status code

       "HTTP/" 1*DIGIT "." 1*DIGIT SP 3DIGIT SP

   (e.g., "HTTP/1.0 200 "), the presence of that expression is
   sufficient to differentiate a Full-Response from a Simple-Response.
   Although the Simple-Response format may allow such an expression to
   occur, most HTTP/0.9 servers are limited to responses of type
   "text/html" and therefore would never generate such a response.

6.1.1 Status Code and Reason Phrase

   The Status-Code element is a 3-digit integer result code of the
   attempt to understand and satisfy the request.

   The first digit of the Status-Code defines the class of response. The
   last two digits do not have any categorization role. There are 5
   values for the first digit:

      o 1xx: Informational - Not used, but reserved for future use

      o 2xx: Success - The action was successfully received,
             understood, and accepted.

      o 3xx: Redirection - Further action must be taken in order to
             complete the request

      o 4xx: Client Error - The request contains bad syntax or cannot
             be fulfilled

      o 5xx: Server Error - The server failed to fulfill an apparently
             valid request

   The individual values of the numeric status codes defined for
   HTTP/1.0, and an example set of corresponding Reason-Phrase's, are
   presented below. The reason phrases listed here are only recommended
   -- they may be replaced by local equivalents without affecting the
   protocol. These codes are fully defined in Section 9.

       Status-Code    = "200"   ; OK
                      | "201"   ; Created
                      | "202"   ; Accepted
                      | "204"   ; No Content
                      | "301"   ; Moved Permanently
                      | "302"   ; Moved Temporarily
                      | "304"   ; Not Modified
                      | "400"   ; Bad Request
                      | "401"   ; Unauthorized
                      | "403"   ; Forbidden
                      | "404"   ; Not Found
                      | "500"   ; Internal Server Error
                      | "501"   ; Not Implemented
                      | "502"   ; Bad Gateway
                      | "503"   ; Service Unavailable
                      | extension-code

       extension-code = 3DIGIT

       Reason-Phrase  = *<TEXT, excluding CR, LF>

   HTTP status codes are extensible, but the above codes are the only
   ones generally recognized in current practice. HTTP applications are
   not required to understand the meaning of all registered status
   codes, though such understanding is obviously desirable. However,
   applications must understand the class of any status code, as
   indicated by the first digit, and treat any unrecognized response as
   being equivalent to the x00 status code of that class, with the
   exception that an unrecognized response must not be cached. For
   example, if an unrecognized status code of 431 is received by the
   client, it can safely assume that there was something wrong with its
   request and treat the response as if it had received a 400 status
   code. In such cases, user agents should present to the user the
   entity returned with the response, since that entity is likely to
   include human-readable information which will explain the unusual
   status.

6.2  Response Header Fields

   The response header fields allow the server to pass additional
   information about the response which cannot be placed in the Status-
   Line. These header fields give information about the server and about
   further access to the resource identified by the Request-URI.

       Response-Header = Location                ; Section 10.11
                       | Server                  ; Section 10.14
                       | WWW-Authenticate        ; Section 10.16

    Unrecognized header fields are treated as Entity-Header fields.

7.1  Entity Header Fields

   Entity-Header fields define optional metainformation about the
   Entity-Body or, if no body is present, about the resource identified
   by the request.

       Entity-Header  = Allow                    ; Section 10.1
                      | Content-Encoding         ; Section 10.3
                      | Content-Length           ; Section 10.4
                      | Content-Type             ; Section 10.5
                      | Expires                  ; Section 10.7
                      | Last-Modified            ; Section 10.10
                      | extension-header

       extension-header = HTTP-header

   The extension-header mechanism allows additional Entity-Header fields
   to be defined without changing the protocol, but these fields cannot
   be assumed to be recognizable by the recipient. Unrecognized header
   fields should be ignored by the recipient and forwarded by proxies.
7.2  Entity Body

   The entity body (if any) sent with an HTTP request or response is in
   a format and encoding defined by the Entity-Header fields.

       Entity-Body    = *OCTET

   An entity body is included with a request message only when the
   request method calls for one. The presence of an entity body in a
   request is signaled by the inclusion of a Content-Length header field
   in the request message headers. HTTP/1.0 requests containing an
   entity body must include a valid Content-Length header field.
      For response messages, whether or not an entity body is included with
   a message is dependent on both the request method and the response
   code. All responses to the HEAD request method must not include a
   body, even though the presence of entity header fields may lead one
   to believe they do. All 1xx (informational), 204 (no content), and
   304 (not modified) responses must not include a body. All other
   responses must include an entity body or a Content-Length header
   field defined with a value of zero (0).

7.2.1 Type

   When an Entity-Body is included with a message, the data type of that
   body is determined via the header fields Content-Type and Content-
   Encoding. These define a two-layer, ordered encoding model:

       entity-body := Content-Encoding( Content-Type( data ) )

   A Content-Type specifies the media type of the underlying data. A
   Content-Encoding may be used to indicate any additional content
   coding applied to the type, usually for the purpose of data
   compression, that is a property of the resource requested. The
   default for the content encoding is none (i.e., the identity
   function).

   Any HTTP/1.0 message containing an entity body should include a
   Content-Type header field defining the media type of that body. If
   and only if the media type is not given by a Content-Type header, as
   is the case for Simple-Response messages, the recipient may attempt
   to guess the media type via inspection of its content and/or the name
   extension(s) of the URL used to identify the resource. If the media
   type remains unknown, the recipient should treat it as type
   "application/octet-stream".