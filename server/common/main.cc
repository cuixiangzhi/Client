
#include <gl/glut.h>
#include <windows.h>

void loop()
{

}

int main(int argc, char** argv)
{
	glutInit(&argc, argv);

	int widthSys = GetSystemMetrics(SM_CXSCREEN);
	int heightSys = GetSystemMetrics(SM_CYSCREEN);

	int widthApp = 960;
	int heightApp = 540;

	int posx = widthSys * 0.5 - widthApp * 0.5;
	int posy = heightSys * 0.5 - heightApp * 0.5;

	glutInitWindowSize(widthApp, heightApp);
	glutInitWindowPosition(posx, posy);
	glutCreateWindow("Unity");

	glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH | GLUT_STENCIL | GLUT_ALPHA);
	glutDisplayFunc(loop);

	const GLubyte* version = glGetString(GL_VERSION);

	glutMainLoop();
}