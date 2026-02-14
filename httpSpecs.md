http rfc spec as of feb 2026

http 1.0 spec - https://datatracker.ietf.org/doc/html/rfc1945
http 1.1 spec - https://datatracker.ietf.org/doc/html/rfc9112
http 2.0 spec - https://datatracker.ietf.org/doc/html/rfc9113
http 3.0 spec - https://datatracker.ietf.org/doc/html/rfc9114


# HTTP/0.9 and HTTP/1.0 Server Requirements Specification
Derived from RFC 1945



# HTTP/0.9 REQUIREMENTS

## Version Detection

**0.9-1.** If the request line does not include HTTP-Version, server MUST treat it as HTTP/0.9 Simple-Request.  
(§3.1, §4.1, §5)

**0.9-2.** Server MUST recognize Simple-Request format: `GET SP Request-URI CRLF` (§4.1, §5)



## Methods

**0.9-3.** Server MUST support GET method in Simple-Request.  
(§4.1, §5)



## Response Format

**0.9-4.** Server MUST send Simple-Response as Entity-Body only (no Status-Line, no headers).  
(§4.1, §6)

**0.9-5.** Server MUST terminate Simple-Response by closing the connection.  
(§6, §7.2.2)



# HTTP/1.0 REQUIREMENTS



# 1. Request-Line Parsing

**1.1.** Server MUST recognize Full-Request format when HTTP-Version is present.  
(§3.1, §5)

**1.2.** Request-Line MUST follow syntax:  `Method SP Request-URI SP HTTP-Version CRL` (§5.1)

**1.3.** Method MUST be parsed as case-sensitive token.  
(§5.1.1)

**1.4.** Server MUST accept extension-method tokens (not limited to GET, HEAD, POST).  
(§5.1.1)

**1.5.** Server SHOULD return 501 if method is unrecognized or not implemented.  
(§5.1.1, §9.5)

**1.6.** Request-URI MUST be parsed as either:
- absoluteURI  
- abs_path  
(§5.1.2)

**1.7.** If abs_path is empty, server MUST treat it as "/".  
(§3.2.2, §5.1.2)

**1.8.** Server MUST decode percent-encoded characters in Request-URI.  
(§5.1.2)



# 2. Response-Line Requirements

**2.1.** Full-Response MUST begin with Status-Line.  
(§4.1, §6.1)

**2.2.** Status-Line MUST follow syntax:  `HTTP-Version SP Status-Code SP Reason-Phrase CRLF` (§6.1)

**2.3.** Status-Code MUST be 3-digit integer.  
(§6.1.1)

**2.4.** Server MUST include HTTP-Version in Status-Line.  
(§6.1)



# 3. Message Structure

**3.1.** HTTP message MUST follow structure: (§4.1)
```
start-line
headers
CRLF
optional Entity-Body
```


**3.2.** Headers MUST be terminated by empty line (CRLF).  
(§4.1)

**3.3.** Entity-Body MUST be treated as raw octet stream.  
(§7.2)



# 4. Header Parsing Requirements

**4.1.** Headers MUST follow syntax: `field-name ":" [ field-value ] CRLF` (§4.2)

**4.2.** Header names MUST be treated as case-insensitive.  
(§4.2)

**4.3.** Server MUST accept header line folding (continuation lines starting with SP or HT).  
(§4.2)

**4.4.** Server MUST accept multiple headers with same name.  
(§4.2)

**4.5.** Server MUST NOT fail when encountering unrecognized headers.  
(§7.1)

**4.6.** Server MUST use CRLF as header line terminator.  
(§2.2)



# 5. Entity Body Framing

## Request Body

**5.1.** Request containing Entity-Body MUST include valid Content-Length header.  
(§7.2, §7.2.2)

**5.2.** Server SHOULD respond 400 if request body exists without valid Content-Length.  
(§7.2.2, §8.3)



## Response Body Presence Rules

**5.3.** Responses to HEAD MUST NOT include Entity-Body.  
(§7.2, §8.2)

**5.4.** Responses with status:
- 1xx
- 204
- 304  

MUST NOT include Entity-Body.  
(§7.2)

**5.5.** All other responses MUST include either:
- Entity-Body, OR  
- Content-Length: 0  
(§7.2)



## Response Body Length Determination

**5.6.** If Content-Length present, server MUST use it as Entity-Body length.  
(§7.2.2, §10.4)

**5.7.** If Content-Length absent, response body length MUST be determined by connection close.  
(§7.2.2)



# 6. Content-Length Requirements

**6.1.** Content-Length MUST represent Entity-Body length in octets.  
(§10.4)

**6.2.** Content-Length value MUST be ≥ 0.  
(§10.4)

**6.3.** Content-Length MUST be used for request body framing.  
(§7.2.2)



# 7. Content-Type and Content-Encoding

**7.1.** Content-Type header defines media type of Entity-Body.  
(§7.2.1, §10.5)

**7.2.** Content-Encoding header defines encoding applied to Entity-Body.  
(§7.2.1, §10.3)

**7.3.** Server MUST parse and expose Content-Encoding.  
(§10.3)

**7.4.** Content-Encoding values MUST be treated case-insensitive.  
(§3.5)

**7.5.** Server SHOULD treat gzip ≡ x-gzip and compress ≡ x-compress.  
(§3.5)



# 8. Date Handling

**8.1.** Server MUST accept HTTP-date formats:
- RFC1123-date
- RFC850-date
- asctime-date  
(§3.3)

**8.2.** Server MUST interpret all HTTP-date values as GMT.  
(§3.3)



# 9. Connection Handling

**9.1.** Server SHOULD close connection after sending response.  
(§1.3)

**9.2.** Connection close MUST terminate response body if Content-Length not present.  
(§7.2.2)

**9.3.** Connection close terminates HTTP message.  
(§7.2.2)

# 10. Method-Specific Requirements

**10.1.** Server MUST implement GET semantics (return entity).  
(§8.1)

**10.2.** Server MUST implement HEAD semantics (no body, headers only).  
(§8.2)

**10.3.** POST requests containing body MUST include valid Content-Length.  
(§8.3)





http 0.9 requirements 
1. No http version in the request line => use HTTP/0.9 as a simple request.
2. Support for only get method.
3. After response complete close the connection

    HTTP-message   = Simple-Request | Simple-Response
    Simple-Request  = "GET" SP Request-URI CRLF
    Simple-Response = [ Entity-Body ]

http 1.0 Requirements 

1. Accept datetime in RFC 1123, RFC 850 or ASCII's c asctime, for responses prefer RFC1123. All should be in GTM time 

2. HTTP/1.0 in request line => full-request 

3. For full request responsd with status line + headers + optional body
       HTTP-message   = Full-Request             ; HTTP/1.0 messages
                      | Full-Response

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

        Request-Line = Method SP Request-URI SP HTTP-Version CRLF
        Status-Line = HTTP-Version SP Status-Code SP Reason-Phrase CRLF


4. Methods
    5.1 respond with 501 if method is not implemented
    5.2 HEAD must not return Entity body
    5.3 Expose GET, POST, HEAD + allow user defiend
5. Framing and protocol control headers
    6.1 Content-Length
        6.1.1 On response should be => 0 if content known (0 is no body, size of body in bytes if size is known)
        6.1.2 On response if size of body is not known should not include header and should close the connection after transmitting body
        6.1.3 On POST request must be >=0. If header or body is missing respond with 400
        6.1.4 Other then POST methods must include header with value 0 or Entity body header
    6.2 Content-Encoding on both request and response should either decode or encode with x-gzip or x-compress
    6.3 If header is repeated expose to app layer as a array

6. Server Response framing
    6.1 No body for 1xx, 204, 304 status codes




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

       Sun, 06 Nov 1994 08:49:37 GMT    ; RFC 822, updated by RFC 1123
       Sunday, 06-Nov-94 08:49:37 GMT   ; RFC 850, obsoleted by RFC 1036
       Sun Nov  6 08:49:37 1994         ; ANSI C's asctime() format

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