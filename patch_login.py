import re

with open('src/frontend/src/pages/LoginPage.tsx', 'r') as f:
    content = f.read()

# Add useEffect import
content = content.replace("import { useMemo, useState } from 'react'", "import { useEffect, useMemo, useState } from 'react'")

# Add useEffect inside LoginPage
effect = """
  // Global fallback: if we land on /login with a sessionCode (e.g. backend misconfigured redirect URL),
  // we can still exchange the session and log in!
  useEffect(() => {
    const params = new URLSearchParams(location.search)
    const sessionCode = params.get('sessionCode')
    if (sessionCode && !isAuthenticated) {
      navigate('/auth/callback/google' + location.search, { replace: true })
    }
  }, [location.search, navigate, isAuthenticated])
"""

content = content.replace("const [isRedirecting, setIsRedirecting] = useState(false)", "const [isRedirecting, setIsRedirecting] = useState(false)\n" + effect)

# We need isAuthenticated and navigate
content = content.replace("const { login, isLoading } = useAuth()", "const { login, isLoading, isAuthenticated } = useAuth()")
content = content.replace("import { Link, useLocation } from 'react-router-dom'", "import { Link, useLocation, useNavigate } from 'react-router-dom'")
content = content.replace("const location = useLocation()", "const location = useLocation()\n  const navigate = useNavigate()")

with open('src/frontend/src/pages/LoginPage.tsx', 'w') as f:
    f.write(content)
