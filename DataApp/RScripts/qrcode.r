library(qrcode)
type <- 'auth'
url <- 'deftgeneralstorage'
key <- 'YHsWGGl3lYDhFRGkkDxp1JvHsrf5tp9ySfIjPZZ75KZlxuiLDT0LBILsVr/Xw0H0yIHMDDnMMYmWBtcGaH2+Sw=='
code <- paste(type, url, key, sep='|')
code
#qrcode_gen(code)
qrcode_gen(code,softLimitFlag = TRUE)

type <- 'graph'
ms <- as.character(as.integer(as.POSIXct(Sys.time())))
container <- 'holograph'
blob <- 'irisTest_test.hgd'
code <- paste(type, ms, container, blob, sep='|')
code
#qrcode_gen(code)
qrcode_gen(code,softLimitFlag = TRUE)

