<xsl:stylesheet
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
version="1.0">
	<xsl:output method='html' indent='yes'/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Schedule</title>
			</head>

			<body>
				<h1>Schedule</h1>
				<table border ="2">
					<TR>
						<TH> Teacher </TH>
						<TH> Teacher Infromation </TH>
						<TH> Subject </TH>
						<TH> Auditorium </TH>
						<TH> Group </TH>
						<TH> Students </TH>
					</TR>
					<xsl:for-each select="schedule/teacher">
						<TR>
							<TD>
								<xsl:value-of select="@name"/>
							</TD>
							<TD>
								<xsl:if test="@faculty">
									Faculty: <xsl:value-of select="@faculty"/><br/>
								</xsl:if>
								<xsl:if test="@department">
									Department: <xsl:value-of select="@department"/><br/>
								</xsl:if>
								<xsl:if test="@position">
									Position: <xsl:value-of select="@position"/><br/>
								</xsl:if>
								<xsl:if test="@email">
									Email: <xsl:value-of select="@email"/><br/>
								</xsl:if>
							</TD>

							<TD>
								<xsl:value-of select="studyPlan/subject/@name"/>
								<br/>
								Hours: <xsl:value-of select="studyPlan/subject/@hours"/>
							</TD>

							<TD>
								<xsl:for-each select="studyPlan/subject/auditorium">
									<xsl:value-of select="."/>
									<br/>
								</xsl:for-each>
							</TD>

							<TD>
								<xsl:value-of select="studentGroup/@name"/>
							</TD>

							<TD>
								<xsl:for-each select="studentGroup/student">
									<xsl:value-of select="."/>
									<br/>
								</xsl:for-each>
							</TD>
						</TR>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
