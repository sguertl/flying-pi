class WifiData:
        def __init__(self, correctness, timedif, bandwidth):
            self.Correctness = 1
            if correctness == False:
                    self.Correctness = 0
            self.Timedif = timedif
            self.Bandwidth = bandwidth

        def getCorrectness(self):
            if self.Correctness == True:
                    return 1
            return 0

        def getTimedif(self):
            return self.Timedif

        def getBandwidth(self):
            return self.Bandwidth

        def getAll(self):
            return str(self.Correctness) + ";" + str(self.Timedif) + ";" + str(self.Bandwidth) + "-"
