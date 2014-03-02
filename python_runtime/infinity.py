class Inf:
    """ Singleton class: the single instance "INFINITY" stands for infinity.
        By Jean-Sebastien and Hans Vangheluwe"""
    __instantiated = False
    def __init__(self):
        if self.__instantiated:
            raise NotImplementedError, "singleton class already instantiated"
        self.__instantiatiated = True

    def __deepcopy__(self, memo):
        """ When deepcopy (in the copy module) makes a deep copy,
                an instance of Inf should NOT be cloned as Inf would
                then no longer be a Singleton.
                Rather, deepcopy should return a reference to the
                unique (singleton) instance. 
                With this approach
                 inf = INFINITY
                 inf_copy = deepcopy(inf)
                 inf == inf_copy
        """
        return self

    def __add__(self, other):
        """ INFINITY + x = INFINITY """
        return self

    def __sub__(self, other):
        """ INFINITY - x = INFINITY (if x != INF), or NaN (if x == INFINITY) """
        if other == self:
            raise ValueError, "INFINITY - INFINITY gives NaN (not defined)"
        return self

    def __radd__(self, other):
        """ x + INFINITY = INFINITY """
        return self

    def __rsub__(self, other):
        """ x - INFINITY = -INFINITY (if x != INFINITY), or NaN (if x == INFINITY) """
        if other == self:
            raise ValueError, "INFINITY - INFINITY gives NaN (not defined)"
        raise ValueError, "x - INFINITY gives MINUS_INFINITY (not defined)"

    def __abs__(self):
        """ abs(INFINITY) = INFINITY -- absolute value """
        return self

#    def __cmp__(self, other):
#        if other is self:
#            return 0
#        else:
#            return 1

    def __eq__(self, other):
        if other is self:
            return True
        else:
            return False

    def __ne__(self, other):
        if other is self:
            return False
        else:
            return True 

    def __lt__(self, other):
        return False

    def __le__(self, other):
        if other is self:
            return True
        else:
            return False

    def __gt__(self, other):
        if other is self:
            return False
        else:
            return True 

    def __ge__(self, other):
        return True

    def __repr__(self):
        return "+INFINITY"

# Instantiate singleton:        
INFINITY = Inf()

