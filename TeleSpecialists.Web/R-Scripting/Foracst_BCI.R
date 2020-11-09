
if(nzchar(system.file(package = "forecast"))==FALSE) #Checking forecast availability
  install.packages("forecast",verbose = FALSE) #If not, install it.   
suppressWarnings(library(forecast)) # loading package
forecast_testF <- function(){ #Function starts.

mydata <- read.csv("C:/Users/user/Desktop/Project_Updated/Telespecialists-15-09-2020/Telespecialists/TeleSpecialists.Web/R-Scripting/Monthly_Forcasting.csv")

  data <- ts(mydata[,-1] , start=c(1,1), frequency = 7) #Converting to time series object
  results <- sapply(data, function(x,h_=61, n=30){
    fc <- holt(x, start=c(1,1),damped=TRUE, h=h_)
    fc1 <- hw(x,start=c(1,1), damped=TRUE, seasonal="additive", h = h_)
    #fc2 <-hw(x,start=c(1,1), damped=TRUE, seasonal="multiplicative", h=24)
    xx <- list(fc,fc1) #collecting all models into a list
    res <- do.call(rbind, lapply(xx, function(x) accuracy(x))) #I am using accuracy() to access to RMSE (second column)
    idx <- which.min(res[,2]) # which is minimun 
    #xx[[idx]]$mean # parameters of the model
    c(sum(xx[[idx]]$mean[1:n]),sum(xx[[idx]]$mean[(n+1):h_])) 
  })
  

results[results<0] <- 0
  r2 <- as.data.frame(t(results))
  r2 <- cbind(gsub(pattern = "\\.", " ", row.names(r2)),r2) #changing dot by space
  rownames(r2) <- NULL 
  colnames(r2) <- c("Hospital", "Forecasting 1st Month", "Forecasting 2nd Month")
  mypersonaldata <- write.csv(r2)
  return(mypersonaldata)


}
forecast_testF()